<%@ Page language="c#" Codebehind="HtmlButtonTestPage.aspx.cs" AutoEventWireup="false" Inherits="NUnitAspTestPages.HtmlTester.HtmlButtonTestPage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>HtmlButtonTestPage</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio 7.0">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	</head>
	<body>
		<form id="HtmlButtonTestPage" method="post" action="../RedirectionTarget.aspx">
			<button id="button" name="buttonName" value="buttonValue">This is a <i>fancy</i> button.</button>
			<button id="disabledButton" disabled="true">Disabled</button> <button id="noNameButton">
				This button has no "name" attribute.</button> <input type="hidden" name="buttonName" value="duplicate name" />
		</form>
	</body>
</html>
