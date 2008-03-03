using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

namespace ExecParse
{
    public class ExecParse : Exec
    {

        private delegate void ComplexLog(   string          subcategory,
                                            string          errorCode,
                                            string          helpKeyword,
                                            string          file,
                                            int             lineNumber,
                                            int             columnNumber,
                                            int             endLineNumber,
                                            int             endColumnNumber,
                                            string          message,
                                            params object[] messageArgs);

        StringBuilder   _outputBuilder;
        string          _output;
        string          _configuration;
        XmlDocument     _xmlConfiguration;
        bool            _errorCausesFail;

        public string Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }

        public bool ErrorCausesFail
        {
            get { return _errorCausesFail; }
            set { _errorCausesFail = value; }
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            _outputBuilder.AppendLine(singleLine);
            base.LogEventsFromTextOutput(singleLine, messageImportance);
        }

        public override bool Execute()
        {
            _outputBuilder = new StringBuilder();
            bool execPass = base.Execute();

            ParseOutput(_outputBuilder.ToString());
            return ModifiedPass(execPass);
        }

        private void LoadConfiguration()
        {
            _xmlConfiguration = new XmlDocument();
            string configurationXml = "<xml>" + _configuration + "</xml>";
            Regex namespaceFinder = new Regex("xmlns(:[^=]*?)?=['\"][^'\"]*?['\"]");
            string configurationWithoutNamespaces = namespaceFinder.Replace(configurationXml, "");
            _xmlConfiguration.LoadXml(configurationWithoutNamespaces);
        }

        private int ConvertToInt32OrZero(string potentialNumber)
        {
            int convertedInt32;
            return int.TryParse(potentialNumber, out convertedInt32) ? convertedInt32 : 0;
        }

        private Regex ExtractSearch(XmlElement configurationElement)
        {
            return new Regex(configurationElement.SelectSingleNode("Search").InnerText);
        }

        private Regex ExtractReplaceSearch(XmlElement configurationElement)
        {
            XmlNode replaceSearch = configurationElement.SelectSingleNode("ReplaceSearch");
            if (replaceSearch != null)
            {
                return new Regex(replaceSearch.InnerText);
            }
            else
            {
                return ExtractSearch(configurationElement);
            }
        }

        private string ExtractReplacement(XmlElement xmlElement,
                                            string      nodeName)
        {
            XmlNode replacement = xmlElement.SelectSingleNode(nodeName);
            return (replacement == null) ? null : replacement.InnerText;
        }

        private void ParseComplexElement(string elementName, ComplexLog log)
        {
            XmlNodeList elements = _xmlConfiguration.SelectNodes("xml/" + elementName);
            foreach (XmlElement element in elements)
            {
                Regex search = ExtractSearch(element);
                Regex replaceSearch = ExtractReplaceSearch(element);
                string messageReplacement = ExtractReplacement(element, "Message");
                string codeReplacement = ExtractReplacement(element, "Code");
                string fileReplacement = ExtractReplacement(element, "File");
                string helpKeywordReplacement = ExtractReplacement(element, "HelpKeyword");
                string subcategoryReplacement = ExtractReplacement(element, "Subcategory");
                string lineNumberReplacement = ExtractReplacement(element, "LineNumber");
                string endLineNumberReplacement = ExtractReplacement(element, "EndLineNumber");
                string columnNumberReplacement = ExtractReplacement(element, "ColumnNumber");
                string endColumnNumberReplacement = ExtractReplacement(element, "EndColumnNumber");
                MatchCollection matches = search.Matches(_output);

                foreach (Match match in matches)
                {
                    string message = (messageReplacement == null) ? string.Empty : replaceSearch.Replace(match.Value, messageReplacement);
                    string code = (codeReplacement == null) ? string.Empty : replaceSearch.Replace(match.Value, codeReplacement);
                    string file = (fileReplacement == null) ? string.Empty : replaceSearch.Replace(match.Value, fileReplacement);
                    string helpKeyword = (helpKeywordReplacement == null) ? string.Empty : replaceSearch.Replace(match.Value, helpKeywordReplacement);
                    string subcategory = (subcategoryReplacement == null) ? string.Empty : replaceSearch.Replace(match.Value, subcategoryReplacement);
                    int lineNumber = (lineNumberReplacement == null) ? 0 : ConvertToInt32OrZero(replaceSearch.Replace(match.Value, lineNumberReplacement));
                    int endLineNumber = (endLineNumberReplacement == null) ? 0 : ConvertToInt32OrZero(replaceSearch.Replace(match.Value, endLineNumberReplacement));
                    int columnNumber = (columnNumberReplacement == null) ? 0 : ConvertToInt32OrZero(replaceSearch.Replace(match.Value, columnNumberReplacement));
                    int endColumnNumber = (endColumnNumberReplacement == null) ? 0 : ConvertToInt32OrZero(replaceSearch.Replace(match.Value, endColumnNumberReplacement));

                    // if we have a file and a line-number, but no column number, default the column to zero
                    //  so the IDE will auto-jump to the line in the file
                    if (!string.IsNullOrEmpty(file) && (lineNumber != 0) && (columnNumber == 0))
                    {
                        columnNumber = 1;
                    }

                    log(subcategory, code, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message);
                }
            }
        }

        private void ParseMessages()
        {
            XmlNodeList elements = _xmlConfiguration.SelectNodes("xml/Message");
            foreach (XmlElement element in elements)
            {
                Regex search = ExtractSearch(element);
                Regex replaceSearch = ExtractReplaceSearch(element);
                string messageReplacement = ExtractReplacement(element, "Message");
                string importanceReplacement = ExtractReplacement(element, "Importance");
                MatchCollection matches = search.Matches(_output);

                foreach (Match match in matches)
                {
                    string message = (messageReplacement == null) ? string.Empty : replaceSearch.Replace(match.Value, messageReplacement);
                    MessageImportance importance = string.IsNullOrEmpty(importanceReplacement) ? MessageImportance.Low : (MessageImportance)Enum.Parse(typeof(MessageImportance), replaceSearch.Replace(match.Value, importanceReplacement), true);

                    Log.LogMessage(importance, message);
                }
            }
        }

        public void ParseOutput(string outputFromExecution)
        {
            _output = outputFromExecution;
            LoadConfiguration();

            ParseComplexElement("Error", new ComplexLog(Log.LogError));
            ParseComplexElement("Warning", new ComplexLog(Log.LogWarning));
            ParseMessages();
        }

        public bool ModifiedPass(bool originalPassed)
        {
            return originalPassed && (!_errorCausesFail || !Log.HasLoggedErrors);
        }

    }
}
