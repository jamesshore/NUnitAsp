/********************************************************************************************************************
'
' Copyright (c) 2002, Brian Knowles, Jim Little
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
' THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
'
'*******************************************************************************************************************/

using System;

namespace NUnit.Extensions.Asp.Test
{
	public class HtmlTagTest : NUnitAspTestCase
	{
		public HtmlTagTest(string name) : base(name)
		{
		}

		private void RunAttributeTest(string tagText, string expectedValue)
		{
			HtmlTag tag = new HtmlTag(tagText);
			AssertEquals("attribute", expectedValue, tag.GetAttributeValue("foo"));
		}

		public void TestGetAttributeSingleQuotes()
		{
			RunAttributeTest("<tag foo='bar' />", "bar");
		}

		public void TestGetAttributeDoubleQuotes()
		{
			RunAttributeTest("<tag foo=\"bar\" />", "bar");
		}

		public void TestGetAttributeEmbeddedSingleQuotes()
		{
			RunAttributeTest("<tag foo=\"bar'quote\" />", "bar'quote");
		}			

		public void TestGetAttributeEmbeddedDoubleQuotes()
		{
			RunAttributeTest("<tag foo='bar\"quote' />", "bar\"quote");
		}

		public void TestGetAttributeMultipleLines()
		{
			RunAttributeTest("<tag \nfoo='bar'\n />", "bar");
		}

		public void TestGetAttributeNoValueSingleQuotes()
		{
			RunAttributeTest("<tag foo='' />", "");
		}

		public void TestGetAttributeNoValueDoubleQuotes()
		{
			RunAttributeTest("<tag foo=\"\" />", "");
		}

		public void TestGetAttributeNoAttribute()
		{
			RunAttributeTest("<tag />", null);
		}
	}
}
