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

        public string Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            _outputBuilder.AppendLine(singleLine);
            base.LogEventsFromTextOutput(singleLine, messageImportance);
        }

        public override bool Execute()
        {
            _outputBuilder = new StringBuilder();
            bool pass = base.Execute();

            ParseOutput(_outputBuilder.ToString());
            return pass;
        }

        private void LoadConfiguration()
        {
            _xmlConfiguration = new XmlDocument();
            Regex namespaceFinder = new Regex("xmlns(:[^=]*?)?=['\"][^'\"]*?['\"]");
            string configurationWithoutNamespaces = namespaceFinder.Replace(_configuration, "");
            _xmlConfiguration.LoadXml("<xml>" + configurationWithoutNamespaces + "</xml>");
        }

        private int ConvertToInt32OrZero(string potentialNumber)
        {
            int convertedInt32;
            return int.TryParse(potentialNumber, out convertedInt32) ? convertedInt32 : 0;
        }

        private string ExtractReplacement(  XmlElement  xmlElement,
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
                Regex search = new Regex(element.SelectSingleNode("Search").InnerText);
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
                    string message = (messageReplacement == null) ? string.Empty : search.Replace(match.Value, messageReplacement);
                    string code = (codeReplacement == null) ? string.Empty : search.Replace(match.Value, codeReplacement);
                    string file = (fileReplacement == null) ? string.Empty : search.Replace(match.Value, fileReplacement);
                    string helpKeyword = (helpKeywordReplacement == null) ? string.Empty : search.Replace(match.Value, helpKeywordReplacement);
                    string subcategory = (subcategoryReplacement == null) ? string.Empty : search.Replace(match.Value, subcategoryReplacement);
                    int lineNumber = (lineNumberReplacement == null) ? 0 : ConvertToInt32OrZero(search.Replace(match.Value, lineNumberReplacement));
                    int endLineNumber = (endLineNumberReplacement == null) ? 0 : ConvertToInt32OrZero(search.Replace(match.Value, endLineNumberReplacement));
                    int columnNumber = (columnNumberReplacement == null) ? 0 : ConvertToInt32OrZero(search.Replace(match.Value, columnNumberReplacement));
                    int endColumnNumber = (endColumnNumberReplacement == null) ? 0 : ConvertToInt32OrZero(search.Replace(match.Value, endColumnNumberReplacement));

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
                Regex search = new Regex(element.SelectSingleNode("Search").InnerText);
                string messageReplacement = ExtractReplacement(element, "Message");
                string importanceReplacement = ExtractReplacement(element, "Importance");
                MatchCollection matches = search.Matches(_output);

                foreach (Match match in matches)
                {
                    string message = (messageReplacement == null) ? string.Empty : search.Replace(match.Value, messageReplacement);
                    MessageImportance importance = string.IsNullOrEmpty(importanceReplacement) ? MessageImportance.Low : (MessageImportance)Enum.Parse(typeof(MessageImportance), search.Replace(match.Value, importanceReplacement), true);

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

    }
}
