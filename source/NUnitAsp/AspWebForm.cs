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
using System.Xml;
using NUnit.Framework;

namespace NUnit.Extensions.Asp
{

	public class AspWebForm : Control
	{
		HttpClient browser;

		public AspWebForm(HttpClient browser)
		{
			this.browser = browser;
		}

		public override bool HasElement(string aspId)
		{
			return (GetElementInternal(aspId) != null);
		}

		internal override XmlElement GetElement(string aspId)
		{
			XmlElement element = GetElementInternal(aspId);
			if (element == null) throw new ElementNotVisibleException("Couldn't find " + aspId + " on " + Description);
			return element;
		}

		private XmlElement GetElementInternal(string aspId)
		{
			return browser.CurrentPage.GetElementById(aspId);
		}

		internal override void EnterInputValue(string name, string value)
		{
			browser.SetFormVariable(name, value);
		}

		internal override void Submit()
		{
			browser.SubmitForm(GetAttributeValue("action"), GetAttributeValue("method"));
		}

		public override string Description
		{
			get
			{
				return "web form '" + AspId + "'";
			}
		}

		private XmlElement Element
		{
			get
			{
				XmlNodeList formNodes = browser.CurrentPage.GetElementsByTagName("form");
				Assertion.AssertEquals("page form elements", 1, formNodes.Count);
				return (XmlElement)formNodes[0];
			}
		}

		private string GetAttributeValue(string name) 
		{
			XmlAttribute attribute = Element.Attributes[name];
			if (attribute == null) throw new AttributeMissingException(name, Description);
			return attribute.Value;
		}

		public string AspId
		{
			get
			{
				return GetAttributeValue("id");
			}
		}

		private class ElementNotVisibleException : ApplicationException
		{
			internal ElementNotVisibleException(string message) : base(message)
			{
			}
		}
	}
}