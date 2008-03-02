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
            Assert.AreEqual(0, buildEngine._warnings.Count);
            Assert.AreEqual(0, buildEngine._messages.Count);
            Assert.AreEqual("Message='', Code='', File='ExecParse.cs', HelpKeyword='', Subcategory='Second', LineNumber=23, EndLineNumber=0, ColumnNumber=1, EndColumnNumber=0", buildEngine._errors[0]);
            Assert.AreEqual("Message='my message', Code='5', File='ExecParse.cs', HelpKeyword='banana', Subcategory='Fourth', LineNumber=1, EndLineNumber=2, ColumnNumber=3, EndColumnNumber=0", buildEngine._errors[1]);
        }

        [Test]
        public void TestDefaultColumnIsOne()
        {
            BuildEngineStub buildEngine = new BuildEngineStub();
            ExecParse task = new ExecParse();
            task.BuildEngine = buildEngine;

            task.Configuration = @"
                <Error>
                    <Search>(\b\w*?)\s+?\b\w*?\s+?fail:(.*?):line (\d*)[^\d]((.*) endline (\d+) column1 (\d+) column2 (.+) code (\d+) keyword (.+?)\s)?</Search>
                    <File>$2</File>
                    <LineNumber>$3</LineNumber>
                </Error>";

            string output =
                @"First line;
                Second line fail:ExecParse.cs:line 23
                Third Line";

            task.ParseOutput(output);

            Assert.AreEqual(1, buildEngine._errors.Count);
            Assert.AreEqual(0, buildEngine._warnings.Count);
            Assert.AreEqual(0, buildEngine._messages.Count);
            Assert.AreEqual("Message='', Code='', File='ExecParse.cs', HelpKeyword='', Subcategory='', LineNumber=23, EndLineNumber=0, ColumnNumber=1, EndColumnNumber=0", buildEngine._errors[0]);
        }

        [Test]
        public void TestErrorMultipleLine()
        {
            BuildEngineStub buildEngine = new BuildEngineStub();
            ExecParse task = new ExecParse();
            task.BuildEngine = buildEngine;

            task.Configuration = @"
                <Error>
                    <Search>fail:[\s\S]*?(\w*) line:</Search>
                    <Message>$1</Message>
                </Error>";

            string output =
                @"First line;
                Second line fail:ExecParse.cs:line 23
                Third line:
                Fourth line fail:ExecParse.cs:line 1 my message endline 2 column1 3 column2 x code 5 keyword banana
                Fifth line.";

            task.ParseOutput(output);

            Assert.AreEqual(1, buildEngine._errors.Count);
            Assert.AreEqual(0, buildEngine._warnings.Count);
            Assert.AreEqual(0, buildEngine._messages.Count);
            Assert.AreEqual("Message='Third', Code='', File='', HelpKeyword='', Subcategory='', LineNumber=0, EndLineNumber=0, ColumnNumber=0, EndColumnNumber=0", buildEngine._errors[0]);
        }

        [Test]
        public void TestMultipleErrorSearch()
        {
            BuildEngineStub buildEngine = new BuildEngineStub();
            ExecParse task = new ExecParse();
            task.BuildEngine = buildEngine;

            task.Configuration = @"
                <Error>
                    <Search>line (\d):</Search>
                    <Message>message search 1 = $1</Message>
                </Error>
                <Error>
                    <Search>(\d) line:</Search>
                    <Message>message search 2 = $1</Message>
                </Error>";

            string output =
                @"line 1:
                2 line:
                Third line.";

            task.ParseOutput(output);

            Assert.AreEqual(2, buildEngine._errors.Count);
            Assert.AreEqual(0, buildEngine._warnings.Count);
            Assert.AreEqual(0, buildEngine._messages.Count);
            Assert.AreEqual("Message='message search 1 = 1', Code='', File='', HelpKeyword='', Subcategory='', LineNumber=0, EndLineNumber=0, ColumnNumber=0, EndColumnNumber=0", buildEngine._errors[0]);
            Assert.AreEqual("Message='message search 2 = 2', Code='', File='', HelpKeyword='', Subcategory='', LineNumber=0, EndLineNumber=0, ColumnNumber=0, EndColumnNumber=0", buildEngine._errors[1]);
        }

        [Test]
        public void TestWarning()
        {
            BuildEngineStub buildEngine = new BuildEngineStub();
            ExecParse task = new ExecParse();
            task.BuildEngine = buildEngine;

            task.Configuration = @"
                <Warning>
                    <Search>fail:([\s\S]*?):</Search>
                    <Message>$1</Message>
                </Warning>";

            string output =
                @"First line;
                Second line fail:ExecParse.cs:line 23
                Third line.";

            task.ParseOutput(output);

            Assert.AreEqual(0, buildEngine._errors.Count);
            Assert.AreEqual(1, buildEngine._warnings.Count);
            Assert.AreEqual(0, buildEngine._messages.Count);
            Assert.AreEqual("Message='ExecParse.cs', Code='', File='BuildEngineStub', HelpKeyword='', Subcategory='', LineNumber=0, EndLineNumber=0, ColumnNumber=0, EndColumnNumber=0", buildEngine._warnings[0]);
        }

        [Test]
        public void TestMessage()
        {
            BuildEngineStub buildEngine = new BuildEngineStub();
            ExecParse task = new ExecParse();
            task.BuildEngine = buildEngine;

            task.Configuration = @"
                <Message>
                    <Search>fail:([\s\S]*?):</Search>
                    <Message>$1</Message>
                    <Importance>high</Importance>
                </Message>";

            string output =
                @"First line;
                Second line fail:ExecParse.cs:line 23
                Third line.";

            task.ParseOutput(output);

            Assert.AreEqual(0, buildEngine._errors.Count);
            Assert.AreEqual(0, buildEngine._warnings.Count);
            Assert.AreEqual(1, buildEngine._messages.Count);
            Assert.AreEqual("Message='ExecParse.cs', Importance='High'", buildEngine._messages[0]);
        }

        [Test]
        public void TestMultipleMessageSearch()
        {
            BuildEngineStub buildEngine = new BuildEngineStub();
            ExecParse task = new ExecParse();
            task.BuildEngine = buildEngine;

            task.Configuration = @"
                <Message>
                    <Search>line (\d):</Search>
                    <Message>message search 1 = $1</Message>
                </Message>
                <Message>
                    <Search>(\d) line:</Search>
                    <Message>message search 2 = $1</Message>
                </Message>";

            string output =
                @"line 1:
                2 line:
                Third line.";

            task.ParseOutput(output);

            Assert.AreEqual(0, buildEngine._errors.Count);
            Assert.AreEqual(0, buildEngine._warnings.Count);
            Assert.AreEqual(2, buildEngine._messages.Count);
            Assert.AreEqual("Message='message search 1 = 1', Importance='Low'", buildEngine._messages[0]);
            Assert.AreEqual("Message='message search 2 = 2', Importance='Low'", buildEngine._messages[1]);
        }

        [Test]
        public void TestConfigurationWithNamespace()
        {
            BuildEngineStub buildEngine = new BuildEngineStub();
            ExecParse task = new ExecParse();
            task.BuildEngine = buildEngine;

            task.Configuration = @"
                <Message xmlns='http://my.namespace.com/test' xmlns:tst=""http://my.namespace.com/test"">
                    <Search>fail:([\s\S]*?):</Search>
                    <Message>$1</Message>
                </Message>";

            string output =
                @"First line;
                Second line fail:ExecParse.cs:line 23
                Third line.";

            task.ParseOutput(output);

            Assert.AreEqual(0, buildEngine._errors.Count);
            Assert.AreEqual(0, buildEngine._warnings.Count);
            Assert.AreEqual(1, buildEngine._messages.Count);
            Assert.AreEqual("Message='ExecParse.cs', Importance='Low'", buildEngine._messages[0]);
        }

        [Test]
        public void TestBlankConfiguration()
        {
            BuildEngineStub buildEngine = new BuildEngineStub();
            ExecParse task = new ExecParse();
            task.BuildEngine = buildEngine;

            task.ParseOutput("");

            Assert.AreEqual(0, buildEngine._errors.Count);
            Assert.AreEqual(0, buildEngine._warnings.Count);
            Assert.AreEqual(0, buildEngine._messages.Count);
        }

    }
}
