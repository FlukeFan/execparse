using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace ExecParse.Test
{

    [TestFixture]
    public class ExecParseTest
    {

        [Test]
        public void TestParseTwoErrors()
        {
            BuildEngineStub buildEngine = new BuildEngineStub();
            ExecParse task = new ExecParse();
            task.BuildEngine = buildEngine;

            task.Configuration = @"
                <Error>
                    <Search>(\b\w*?)\s+?\b\w*?\s+?fail:(.*?):line (\d*)[^\d]((.*) endline (\d+) column1 (\d+) column2 (.+) code (\d+) keyword (.+?)\s)?</Search>
                    <Message>$5</Message>
                    <Code>$9</Code>
                    <File>$2</File>
                    <HelpKeyword>$10</HelpKeyword>
                    <Subcategory>$1</Subcategory>
                    <LineNumber>$3</LineNumber>
                    <EndLineNumber>$6</EndLineNumber>
                    <ColumnNumber>$7</ColumnNumber>
                    <EndColumnNumber>$8</EndColumnNumber>
                </Error>";

            string output =
                @"First line;
                Second line fail:ExecParse.cs:line 23
                Third line.
                Fourth line fail:ExecParse.cs:line 1 my message endline 2 column1 3 column2 x code 5 keyword banana
                Fifth line.";

            task.ParseOutput(output);

            Assert.AreEqual(2, buildEngine._errors.Count);
            Assert.AreEqual("Message='', Code='', File='ExecParse.cs', HelpKeyword='', Subcategory='Second', LineNumber=23, EndLineNumber=0, ColumnNumber=0, EndColumnNumber=0", buildEngine._errors[0]);
            Assert.AreEqual("Message='my message', Code='5', File='ExecParse.cs', HelpKeyword='banana', Subcategory='Fourth', LineNumber=1, EndLineNumber=2, ColumnNumber=3, EndColumnNumber=0", buildEngine._errors[1]);
        }

    }
}
