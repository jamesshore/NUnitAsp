#region Copyright (c) 2002, 2005 Brian Knowles, Jim Shore
/********************************************************************************************************************
'
' Copyright (c) 2002, 2005, Brian Knowles, Jim Shore
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
#endregion

using System;
using NUnit.Framework;
using NUnit.Extensions.Asp.AspTester;

namespace NUnit.Extensions.Asp.Test.AspTester
{
    [TestFixture]
	public class TextBoxTest : NUnitAspTestCase
	{
		private TextBoxTester textBox;
		private TextBoxTester multiline;
		private TextBoxTester disabled;
		private ButtonTester postback;

		protected override void SetUp()
		{
			textBox = new TextBoxTester("textBox", CurrentWebForm);
			multiline = new TextBoxTester("multiline", CurrentWebForm);
			disabled = new TextBoxTester("disabled", CurrentWebForm);
			postback = new ButtonTester("postback", CurrentWebForm);

			Browser.GetPage(BaseUrl + "AspTester/TextBoxTestPage.aspx");
		}

        [Test]
		public void TestEnabled_True()
		{
			AssertEquals("enabled", true, textBox.Enabled);
		}

        [Test]
		public void TestEnabled_False()
		{
			AssertEquals("enabled", false, disabled.Enabled);
		}

        [Test]
		public void TestText()
		{
			AssertEquals("empty text box", "", textBox.Text);
			textBox.Text = "some text";
			postback.Click();
			AssertEquals("text", "some text", textBox.Text);
		}

        [Test]
		public void TestDefaultMaxLength()
		{
			Assert.AreEqual(0, textBox.MaxLength);
		}

        [Test]
		public void TestText_WhenEmpty()
		{
			AssertEquals("empty text box", "", textBox.Text);
			multiline.Text = "";
			postback.Click();
			AssertEquals("empty multiline text box", "", multiline.Text);
		}

        [Test]
		public void TestText_WhenMultiline()
		{
			AssertEquals("default multiline", "default", multiline.Text);
			multiline.Text="new text";
			postback.Click();
			AssertEquals("multiline text box setting", "new text", multiline.Text);
		}

        [Test]
        [ExpectedException(typeof(ControlDisabledException))]
        public void TestText_WhenDisabled()
		{
			disabled.Text = "some text";
		}
	}
}
