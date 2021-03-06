#region Copyright (c) 2002-2005, Brian Knowles, Jim Shore
/********************************************************************************************************************
'
' Copyright (c) 2002-2005, Brian Knowles, Jim Shore
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
using System.Xml;
using System.Text.RegularExpressions;

namespace NUnit.Extensions.Asp
{
	/// <summary>
	/// Base class for all tag-based controls.  Most people should
	/// extend <see cref="AspTester.AspControlTester" /> or <see cref="HtmlTester.HtmlControlTester"/>.
	/// 
	/// The API for this class will change in future releases.
	/// </summary>
	public abstract class ControlTester : Tester
	{
		private Tester container;
		private string aspId = null;

		/// <summary>
		/// Create a tester that has no ID.  This constructor is for NUnitAsp internal use
		/// and should be avoided.  It will likely change in a future release.
		/// </summary>
		protected ControlTester() : this((string)null)
		{
		}

		/// <summary>
		/// Create a tester that has no ID.  This constructor is for NUnitAsp internal use
		/// and should be avoided.  It will likely change in a future release.
		/// </summary>
		protected ControlTester(Tester container) : this(null, container)
		{
		}

		/// <summary>
		/// <p>Create a tester for a top-level control.  Use this constructor
		/// for testing most controls.  Testers created with this constructor
		/// will test pages loaded by the <see cref="HttpClient.Default"/>
		/// HttpClient.</p>
		/// </summary>
		/// <param name="aspId">The ID of the control to test (look in the
		/// page's ASP.NET source code for the ID).</param>
		public ControlTester(string aspId) : this(aspId, new WebFormTester(HttpClient.Default))
		{
		}

		/// <summary>
		/// Create a tester for a nested control.  Use this constructor when 
		/// the control you are testing is nested within another control,
		/// such as a DataGrid or UserControl.  You should also use this
		/// constructor when you're not using the 
		/// <see cref="HttpClient.Default"/> HttpClient.
		/// </summary>
		/// <param name="aspId">The ID of the control to test (look in the
		/// page's ASP.NET source code for the ID).</param>
		/// <param name="container">A tester for the control's container.  
		/// (In the page's ASP.NET source code, look for the tag that the
		/// control is nested in.  That's probably the control's
		/// container.)</param>
		/// 
		/// <example>
		/// This example demonstrates how to test a label that's inside
		/// of a user control:
		/// 
		/// <code>
		/// UserControlTester user1 = new UserControlTester("user1");
		/// LabelTester label = new LabelTester("label", user1);</code>
		/// </example>
		/// 
		/// <example>This example demonstrates how to use an HttpClient
		/// other than <see cref="HttpClient.Default"/>:
		/// 
		/// <code>
		/// HttpClient myHttpClient = new HttpClient();
		/// WebFormTester webForm = new WebFormTester(myHttpClient);
		/// LabelTester myTester = new LabelTester("id", webForm);</code>
		/// </example>
		public ControlTester(string aspId, Tester container)
		{
			this.aspId = aspId;
			this.container = container;
		}

		/// <summary>
		/// The ASP.NET ID of the control being tested.  It corresponds to the
		/// ID in the ASP.NET source code.
		/// </summary>
		public override string AspId
		{
			get
			{
				WebAssert.NotNull(aspId, "Attempted to access AspId when it was null");
				return aspId;
			}
		}

		/// <summary>
		/// Returns the HTML ID of a child control.  Useful when implementing
		/// testers for container controls that do HTML ID mangling.  This method
		/// is very likely to change in a future release.
		/// </summary>
		protected internal override string GetChildElementHtmlId(string aspId)
		{
			return container.GetChildElementHtmlId(aspId);
		}

		/// <summary>
		/// The browser instance used to load the page containing the form being tested.
		/// </summary>
		protected internal override HttpClient Browser
		{
			get
			{
				return container.Browser;
			}
		}

		/// <summary>
		/// The form containing this control
		/// </summary>
		protected internal override WebFormTester Form
		{
			get
			{
				return container.Form;
			}
		}

		/// <summary>
		/// A human-readable description of the location of the control being tested.
		/// This property describes the location of the control as well as providing
		/// the HTML ID of the control, if present.
		/// </summary>
		public override string HtmlIdAndDescription
		{
			get
			{
				try
				{
					return string.Format("{0} ({1})", HtmlId, Description);
				}
				catch (HtmlTagTester.NoHtmlIdException)
				{
					return Description;
				}
			}
		}

		/// <summary>
		/// A human-readable description of the location of the control.  Unlike
		/// <see cref="HtmlIdAndDescription"/>, this property only describes the
		/// location of the control in the ASP.NET source code.
		/// </summary>
		public override string Description 
		{
			get 
			{
				string controlType = this.GetType().Name;
				return string.Format("{0} '{1}' in {2}", controlType, AspId, container.Description);
			}
		}

		/// <summary>
		/// The HTML ID of the control being tested.  It corresponds to the
		/// ID of the HTML tag rendered by the server.  It's useful for looking at 
		/// raw HTML while debugging.
		/// </summary>
		public override string HtmlId
		{
			get
			{
				return container.GetChildElementHtmlId(AspId);
			}
		}

		/// <summary>
		/// True if the control is disabled.
		/// </summary>
		protected virtual bool IsDisabled
		{
			get
			{
				return Tag.HasAttribute("disabled");
			}
		}

		protected internal void EnterInputValue(string name, string value)
		{
			AssertEnabled();
			Form.Variables.ReplaceAll(name, value);
		}

		protected internal void RemoveInputValue(string name)
		{
			AssertEnabled();
			Form.Variables.RemoveAll(name);
		}

		private void AssertEnabled()
		{
			if (IsDisabled) throw new ControlDisabledException(this);
		}

		/// <summary>
		/// Post the form containing this control to the server.
		/// </summary>
		[Obsolete("Use Form.Submit() instead; this method will be removed in v1.7")]
		protected internal virtual void Submit()
		{
			Form.Submit();
		}

		/// <summary>
		/// Like <see cref="PostBack"/>, but doesn't fail if candidatePostBackScript
		/// doesn't contain a post-back script.
		/// </summary>
		[Obsolete("Use Form.OptionalPostBack() instead; this method will be removed in v1.7")]
		protected void OptionalPostBack(string candidatePostBackScript)
		{
			Form.OptionalPostBack(candidatePostBackScript);
		}

		/// <summary>
		/// Checks a string to see if it contains a post-back script.
		/// Typically you should just use <see cref="OptionalPostBack"/> instead.
		/// </summary>
		[Obsolete("Use Form.IsPostBack() instead; this method will be removed in v1.7")]
		protected bool IsPostBack(string candidatePostBackScript)
		{
			return Form.IsPostBack(candidatePostBackScript);
		}

		/// <summary>
		/// Trigger a post-back.  ASP.NET has a post-back idiom that often shows up
		/// as a Javascript "__doPostBack" call.  This method exists to make it easy to write
		/// testers for controls that do so.  Just take the string that contains the post-
		/// back script and pass it to this method.  Use <see cref="OptionalPostBack"/>
		/// if the script isn't always present.
		/// </summary>
		[Obsolete("Use Form.PostBack() instead; this method will be removed in v1.7")]
		protected void PostBack(string postBackScript)
		{
			Form.PostBack(postBackScript);
		}

		/// <summary>
		/// Trigger a post-back.  If you don't have a post-back script but need to trigger a
		/// post-back, call this method with the appropriate event target and argument.
		/// </summary>
		[Obsolete("Use Form.PostBack() instead; this method will be removed in v1.7")]
		protected void PostBack(string eventTarget, string eventArgument)
		{
			Form.PostBack(eventTarget, eventArgument);
		}
	}

	/// <summary>
	/// Exception: The tester has a bug: it was looking for some HTML and didn't find it.
	/// Report this exception to the author of the tester.
	/// </summary>
	public class ParseException : ApplicationException
	{
		internal ParseException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// Exception: The test is trying to perform a UI operation on a disabled control.
	/// Enable the control in your production code or don't change it in the test.
	/// </summary>
	public class ControlDisabledException : InvalidOperationException
	{
		public ControlDisabledException(ControlTester control) :
			base(GetMessage(control))
		{
		}

		private static string GetMessage(ControlTester control)
		{
			return string.Format(
				"Control {0} (HTML ID: {1}; ASP location: {2}) is disabled",
				control.AspId, control.HtmlId, control.Description);
		}
	}
}
