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
            _xmlConfiguration.LoadXml("<xml>" + _configuration + "</xml>");
        }

        private int ConvertToInt32OrZero(string potentialNumber)
        {
            int convertedInt32;
            return int.TryParse(potentialNumber, out convertedInt32) ? convertedInt32 : 0;
        }

        private string ExtractReplacement(XmlElement xmlElement,
                                            string      nodeName)
        {
            XmlNode replacement = xmlElement.SelectSingleNode(nodeName);
            return (replacement == null) ? null : replacement.InnerText;
        }

        private void ParseErrors()
        {
            XmlNodeList errors = _xmlConfiguration.SelectNodes("//Error");
            foreach (XmlElement error in errors)
            {
                Regex search = new Regex(error.SelectSingleNode("Search").InnerText);
                string messageReplacement = ExtractReplacement(error, "Message");
                string codeReplacement = ExtractReplacement(error, "Code");
                string fileReplacement = ExtractReplacement(error, "File");
                string helpKeywordReplacement = ExtractReplacement(error, "HelpKeyword");
                string subcategoryReplacement = ExtractReplacement(error, "Subcategory");
                string lineNumberReplacement = ExtractReplacement(error, "LineNumber");
                string endLineNumberReplacement = ExtractReplacement(error, "EndLineNumber");
                string columnNumberReplacement = ExtractReplacement(error, "ColumnNumber");
                string endColumnNumberReplacement = ExtractReplacement(error, "EndColumnNumber");
                MatchCollection foundErrors = search.Matches(_output);

                foreach (Match errorMatch in foundErrors)
                {
                    string message = (messageReplacement == null) ? string.Empty : search.Replace(errorMatch.Value, messageReplacement);
                    string code = (codeReplacement == null) ? string.Empty : search.Replace(errorMatch.Value, codeReplacement);
                    string file = (fileReplacement == null) ? string.Empty : search.Replace(errorMatch.Value, fileReplacement);
                    string helpKeyword = (helpKeywordReplacement == null) ? string.Empty : search.Replace(errorMatch.Value, helpKeywordReplacement);
                    string subcategory = (subcategoryReplacement == null) ? string.Empty : search.Replace(errorMatch.Value, subcategoryReplacement);
                    int lineNumber = (lineNumberReplacement == null) ? 0 : ConvertToInt32OrZero(search.Replace(errorMatch.Value, lineNumberReplacement));
                    int endLineNumber = (endLineNumberReplacement == null) ? 0 : ConvertToInt32OrZero(search.Replace(errorMatch.Value, endLineNumberReplacement));
                    int columnNumber = (columnNumberReplacement == null) ? 0 : ConvertToInt32OrZero(search.Replace(errorMatch.Value, columnNumberReplacement));
                    int endColumnNumber = (endColumnNumberReplacement == null) ? 0 : ConvertToInt32OrZero(search.Replace(errorMatch.Value, endColumnNumberReplacement));

                    Log.LogError(subcategory, code, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message);
                }
            }
        }

        public void ParseOutput(string outputFromExecution)
        {
            _output = outputFromExecution;
            LoadConfiguration();

            ParseErrors();
        }

    }
}
