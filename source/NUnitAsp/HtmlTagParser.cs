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
using System.Text.RegularExpressions;

namespace NUnit.Extensions.Asp
{
	/// <summary>
	/// Capable of parsing individual tags out of a chunk of HTML.
	/// 
	/// Not intended for third-party use.  The API for this class will change in 
	/// future releases.  This class may not be present in future releases.
	/// </summary>
	public class HtmlTagParser
	{
		private string html;

		public HtmlTagParser(string htmlIn)
		{
			html = htmlIn;
		}

		/// <summary>
		/// Returns null if no tag matches.
		/// </summary>
		public string GetTagById(string id)
		{
			string whiteSpace = "\\s*";
			string requiredWhiteSpace = "\\s+";
			string elementName = "(?<name>\\w*)";
			string optionalQuote = "['\"]?";
			string optionalLeadingAttributes = "(.*?\\s)?";
			string optionalTrailingAttributes = "(\\s.*?)?";
			string idPattern = "id" + whiteSpace + "=" + whiteSpace + optionalQuote + id + optionalQuote;
			string backReferenceToElementName = "\\k<name>";

			string basicPattern = "<" + whiteSpace + elementName + requiredWhiteSpace + optionalLeadingAttributes + idPattern + optionalTrailingAttributes + whiteSpace;
			string leafPattern = basicPattern + "/" + whiteSpace + ">";
			string branchPattern = basicPattern + ">.*?<" + whiteSpace + "/" + whiteSpace + backReferenceToElementName + whiteSpace + ">";
			RegexOptions options = RegexOptions.Singleline | RegexOptions.IgnoreCase;

			// check for duplicate ids
			string requiredValueTerminator = "['\"/>"+whiteSpace+"]+";
			string duplicateIdPatttern = String.Format("id{0}={0}{1}{2}{3}", whiteSpace, optionalQuote, id, requiredValueTerminator);
			MatchCollection idMatches = Regex.Matches(html, duplicateIdPatttern, options);
			if (idMatches.Count>1)
			{
				throw new UniqueIdException(id);
			}

			MatchCollection matches = Regex.Matches(html, leafPattern, options);
			if (matches.Count == 0)
			{
				matches = Regex.Matches(html, branchPattern, options);
				if (matches.Count == 0) return null;
			}
			return matches[0].Value;
		}

		public class UniqueIdException : ApplicationException
		{
			public UniqueIdException(string id) : base("HTML ID '" + id + "' is not unique")
			{
			}
		}
	}
}
