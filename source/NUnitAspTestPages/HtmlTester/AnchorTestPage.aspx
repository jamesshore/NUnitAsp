<%@ Page language="c#" Codebehind="AnchorTestPage.aspx.cs" AutoEventWireup="false" Inherits="NUnit.Extensions.Asp.Test.HtmlTester.AnchorTestPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://localhost/NUnitAsp/dtd/xhtml1-transitional.dtd">
<HTML>
	<HEAD>
		<title>AnchorTestPage</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio 7.0" />
		<meta name="CODE_LANGUAGE" Content="C#" />
		<meta name="vs_defaultClientScript" content="JavaScript" />
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
	</HEAD>
	<body>
		<form id="AnchorTestPage" method="post" runat="server">
			<a id="testLink" href="../RedirectionTarget.aspx?a=a&amp;b=b">Click Here</a>
		</form>
	</body>
</HTML>