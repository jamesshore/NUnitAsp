// NAnt - A .NET build tool
// Copyright (C) 2001-2002 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

// Ian MacLean (ian_maclean@another.com)
// Gerry Shaw (gerry_shaw@yahoo.com)

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using System.Xml;

namespace SourceForge.NAnt.Tasks.NUnit {

    /// <summary>Prints detailed information in XML format about running tests.</summary>
    public class XmlResultFormatter : IResultFormatter	{
			   	 
        const string ElementTestSuites = "testsuites";
        const string ElementTestSuite = "testsuite";
        const string ElementTestCase = "testcase";
        const string ElementError = "error";
        const string ElementFailure = "failure";		  
	    
        const string AttributePackage = "package";
        const string AttributeName = "name";
        const string AttributeTime = "time";
        const string AttributeErrors = "errors";
        const string AttributeFailures = "failures";
        const string AttributeTests = "tests";
        const string AttributeType = "type";
        const string AttributeMessage = "message";
        const string AttributeClassname = "classname";
	    	    	    	    
        TextWriter _writer;
	    
        // Document builder members
        XmlDocument _document;	   
        XmlElement _suiteElement;
        XmlElement _currentTest;
	    
        DateTime _testStartTime;
	
        public XmlResultFormatter() {
            _document = new XmlDocument();
        }
	    
        public TextWriter Writer {
            get { return _writer; }
            set { _writer = value; }
        }
        
        //-------------------------------------------------------------
        // IResultFormatter interface methods
        //-------------------------------------------------------------

        /// <summary>Sets the Writer the formatter is supposed to write its results to.</summary>
        public void SetOutput(TextWriter writer) {
            Writer = writer;
        }

        /// <summary>Called when the whole test suite has started.</summary>
        public void StartTestSuite(NUnitTest suite) {
            XmlDeclaration decl = _document.CreateXmlDeclaration("1.0", null, null);            
            _document.AppendChild(decl);
            _suiteElement = _document.CreateElement(ElementTestSuite);
            //
            // if this is a testsuite, use it's name
            //
            string suiteName = suite.Suite.ToString();
            if ( suiteName == null || suiteName == string.Empty )
                suiteName = "test"; 
            _suiteElement.SetAttribute(AttributeName, suiteName );
        }

        /// <summary>Called when the whole test suite has ended.</summary>
        public void EndTestSuite(TestResultExtra result) {
            _suiteElement.SetAttribute(AttributeTests , result.RunCount.ToString());
            double time = result.RunTime;
            time /= 1000D; 
            _suiteElement.SetAttribute(AttributeTime, time.ToString("#####0.000"));
            _document.AppendChild(_suiteElement);

            _suiteElement.SetAttribute(AttributeErrors , result.ErrorCount.ToString()); 
            _suiteElement.SetAttribute(AttributeFailures , result.FailureCount.ToString());
            
            // Send all output to here
            _document.Save(Writer);
            Writer.Flush();
            Writer.Close();
        }

        //-------------------------------------------------------------
        // ITestListener interface methods
        //-------------------------------------------------------------
		
        public void AddError(ITest test, Exception t) {
            FormatError(ElementError, test, t);
        }

        public void AddFailure(ITest test, AssertionFailedError t) {
            FormatError(ElementFailure, test, (Exception)t);
        }

        public void StartTest(ITest test) {
            _testStartTime =  DateTime.Now;
            _currentTest = _document.CreateElement(ElementTestCase);
            _currentTest.SetAttribute(AttributeName, ((TestCase ) test).ToString());
            
            string className = test.GetType().FullName;
            _currentTest.SetAttribute(AttributeClassname, className);

            _suiteElement.AppendChild(_currentTest);
        }

        public void EndTest(ITest test) {
            TimeSpan elapsedTime = DateTime.Now - _testStartTime;
            double time = elapsedTime.Milliseconds;
            time /= 1000D; 
            _currentTest.SetAttribute(AttributeTime, time.ToString("#####0.000") ); 
        }
	    
        private void FormatError(string type, ITest test, Exception t) {
            if (test != null) {
                EndTest(test);
            }

            XmlElement nested = _document.CreateElement(type);
            if (test != null) {
                _currentTest.AppendChild(nested);
            } else {
                _suiteElement.AppendChild(nested);
            }

            string message = t.Message;
            if (message != null && message.Length > 0) {
                nested.SetAttribute( AttributeMessage, message );
            }
            nested.SetAttribute(AttributeType, t.GetType().FullName);

            string trace = t.StackTrace;
                    
            XmlText traceElement = _document.CreateTextNode(t.StackTrace);
            nested.AppendChild(traceElement);
        }
    }			
}
