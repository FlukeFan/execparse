using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Build.Framework;

namespace ExecParse.Test
{
    class BuildEngineStub : IBuildEngine
    {

        public List<string> _errors = new List<string>();
        public List<string> _warnings = new List<string>();
        public List<string> _messages = new List<string>();

        public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int ColumnNumberOfTaskNode
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool ContinueOnError
        {
            get { return true; }
        }

        public int LineNumberOfTaskNode
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            _errors.Add(
                "Message='" + e.Message + "', "
                + "Code='" + e.Code + "', "
                + "File='" + e.File + "', "
                + "HelpKeyword='" + e.HelpKeyword + "', "
                + "Subcategory='" + e.Subcategory + "', "
                + "LineNumber=" + e.LineNumber.ToString() + ", "
                + "EndLineNumber=" + e.EndLineNumber.ToString() + ", "
                + "ColumnNumber=" + e.ColumnNumber.ToString() + ", "
                + "EndColumnNumber=" + e.EndColumnNumber.ToString());
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            _messages.Add(
                "Message='" + e.Message + "', "
                + "Importance='" + e.Importance.ToString() + "'");
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            _warnings.Add(
                "Message='" + e.Message + "', "
                + "Code='" + e.Code + "', "
                + "File='" + e.File + "', "
                + "HelpKeyword='" + e.HelpKeyword + "', "
                + "Subcategory='" + e.Subcategory + "', "
                + "LineNumber=" + e.LineNumber.ToString() + ", "
                + "EndLineNumber=" + e.EndLineNumber.ToString() + ", "
                + "ColumnNumber=" + e.ColumnNumber.ToString() + ", "
                + "EndColumnNumber=" + e.EndColumnNumber.ToString());
        }

        public string ProjectFileOfTaskNode
        {
            get { return "BuildEngineStub"; }
        }

    }
}
