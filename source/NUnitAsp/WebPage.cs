using System;
using System.Xml;
using System.Collections.Specialized;
using System.Web;
using System.Text.RegularExpressions;

namespace NUnit.Extensions.Asp
{
	internal class WebPage
	{
		private string pageText;
		private XmlDocument document = null;
		private NameValueCollection formVariables = new NameValueCollection();

		internal WebPage(string htmlPage)
		{
			pageText = htmlPage;
		}

		internal XmlDocument Document
		{
			get
			{
				try 
				{
					if (document == null)
					{
						document = new XmlDocument();
						document.LoadXml(ConvertToXhtml(pageText));
						ParseDefaultFormVariables();
					}
					return document;
				}
				catch (XmlException e)
				{
					Console.WriteLine(pageText);
					throw e;
				}
			}
		}

		private string ConvertToXhtml(string html)
		{
			// doctype
			html = Regex.Replace(html, @"[<][!]DOCTYPE.*[>]", "<!DOCTYPE HTML PUBLIC \"-//w3c//dtd xhtml 1.0 transitional//en\" \"http://localhost/nunitasp/web/dtd/xhtml1-transitional.dtd\">", RegexOptions.IgnoreCase);

			// lowercase <html>
			html = Regex.Replace(html, @"<html>", "<HTML>");
			html = Regex.Replace(html, @"</html>", "</HTML>");

			// unclosed tags with attributes
			html = Regex.Replace(html, "<(INPUT|IMG|META|LINK|BASE|BGSOUND)([^<]+)(\"| |')>", "<$1$2$3 />", RegexOptions.IgnoreCase);

			// unclosed tags with no attributes
			html = Regex.Replace(html, "<(BR|HR)>", "<$1 />", RegexOptions.IgnoreCase);

			// nowrap (DataGrids)
			html = Regex.Replace(html, "nowrap=\"nowrap\"", "  nowrap=\"TRUE\"  ", RegexOptions.IgnoreCase);
			html = Regex.Replace(html, @"nowrap([^=])", " nowrap=\"true\"$1  ", RegexOptions.IgnoreCase);

			// anchor tags
			html = Regex.Replace(html, "<A ", "<a ");
			html = Regex.Replace(html, "</A>", "</a>");

			return html;
		}

		internal string FormVariables
		{
			get
			{
				string joiner = "";
				string result = "";
				foreach (string key in formVariables.Keys)
				{
					result += joiner + HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(formVariables.Get(key));
					joiner = "&";
				}
				return result;
			}
		}

		private void ParseDefaultFormVariables() 
		{
			formVariables = new NameValueCollection();
			ParseFormHiddenValues();
			ParseFormTextValues("//input[@type='file']");
			ParseFormTextValues("//input[@type='password']");
			ParseFormTextValues("//input[@type='text']");
			ParseFormTextValues("//input[@type='radio'][@checked]");
			ParseFormCheckBoxValues();
			ParseFormTextAreaValues();
			ParseFormSelectValues();
		}

		private void ParseFormHiddenValues() 
		{
			XmlNodeList nodes = Document.SelectNodes("//input[@type='hidden']");
			if (nodes == null) return;

			foreach (XmlNode item in nodes) 
			{
				XmlAttribute name = item.Attributes["name"];
				XmlAttribute aValue = item.Attributes["value"];
				if ((name != null) && (aValue != null)) 
				{
					SetFormVariable(name.Value, aValue.Value);
				}
			}
		}

		private void ParseFormTextValues(string expression) 
		{
			XmlAttribute name;
			XmlAttribute aValue;
			XmlNodeList nodes = Document.SelectNodes(expression);
			if (nodes == null) return;

			foreach (XmlNode item in nodes) 
			{
				name = item.Attributes["name"];
				aValue = item.Attributes["value"];
				if ((name != null) && (aValue != null)) 
				{
					SetFormVariable(name.Value, aValue.Value);
				}
			}
		}

		private void ParseFormCheckBoxValues() 
		{
			XmlAttribute name;
			XmlNodeList nodes = Document.SelectNodes("//input[@type='checkbox'][@checked]");
			if (nodes == null) return;

			foreach (XmlNode item in nodes) 
			{
				name = item.Attributes["name"];
				if (name != null) 
				{
					SetFormVariable(name.Value, "checked");
				}
			}
		}

		private void ParseFormTextAreaValues() 
		{
			XmlAttribute name;
			XmlNodeList nodes = Document.SelectNodes("//textarea");
			if (nodes == null) return;

			foreach (XmlNode item in nodes) 
			{
				name = item.Attributes["name"];
				if (name != null) 
				{
					SetFormVariable(name.Value, item.InnerText.Trim());
				}
			}
		}

		private void ParseFormSelectValues() 
		{
			string expression = "//select";
			XmlAttribute name;
			XmlNode aValue = null;
			XmlNodeList nodes = Document.SelectNodes(expression);

			if (nodes == null) return;

			foreach (XmlNode item in nodes) 
			{
				name = item.Attributes["name"];
				if (name == null) 
				{
					throw new XmlException("A select form element on the page does not have a name.", null);
				}

				// Look for the option that is selected
				foreach (XmlNode child in item.ChildNodes) 
				{
					if (child.Attributes["selected"] != null) 
					{
						aValue = child.Attributes["value"];
					}
				}

				// If there is no value then we will just set it as empty
				if (aValue == null) 
				{
					SetFormVariable(name.Value, String.Empty);
				}
				else 
				{
					SetFormVariable(name.Value, aValue.Value);
				}
			}
		}	

		public void SetFormVariable(string name, string value) 
		{
			formVariables.Remove(name);
			formVariables.Add(name, value);
		}	

		public override string ToString()
		{
			return pageText;
		}
	}
}
