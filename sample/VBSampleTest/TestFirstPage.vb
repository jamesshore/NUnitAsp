'********************************************************************************************************************
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
'*******************************************************************************************************************

Option Strict On

Imports NUnit.Framework
Imports NUnit.Extensions.Asp

Namespace NUnit.Extensions.Asp.VBSample.Test
    Public Class TestFirstPage
        Inherits WebFormTestCase

        Public Overrides ReadOnly Property StartUrl() As String
            Get
                Return "http://localhost/NUnitAsp/VBSamplePages/FirstPage.aspx"
            End Get
        End Property

        Public Overrides ReadOnly Property PageName() As String
            Get
                Return "FirstPage"
            End Get
        End Property

#Region "NUnit Framework Methods"
        Public Sub New(ByVal name As String)
            MyBase.New(name)
        End Sub

        Protected Overrides Sub SetUp()
            MyBase.SetUp()
        End Sub

        Protected Overrides Sub TearDown()
            MyBase.TearDown()
        End Sub
#End Region

        Public Sub TestForm()
            AssertFormExists("Form1")
            AssertTextBoxExists("Name")
            AssertTextBoxValue("Name", "Bob")
            AssertDropDownListExists("Occupation")
            AssertDropDownListSelection("Occupation", "Builder")
            AssertDropDownHasListItem("Occupation", "Senator")
            AssertDropDownHasListItem("Occupation", "Sportscaster")
            AssertSubmitButtonExists("CartoonName")
        End Sub

        Public Sub TestCreateCartoonName()
            AssertLabelExists("BuiltName")
            AssertLabelText("BuiltName", String.Empty)
            _browser.PushButton("CartoonName", "Form1")
            AssertPageName("FirstPage")
            AssertLabelText("BuiltName", "Bob the Builder")
        End Sub

        Public Sub TestCreateDifferentCartoonName()
            AssertLabelExists("BuiltName")
            AssertLabelText("BuiltName", String.Empty)
            _browser.SelectDropDownListItem("Occupation", "Senator")
            _browser.PushButton("CartoonName", "Form1")
            AssertPageName("FirstPage")
            AssertLabelText("BuiltName", "Bob the Senator")
        End Sub

        Public Sub TestNameRequiredValidation()
            _browser.EnterInputValue("Name", String.Empty)
            _browser.PushButton("CartoonName", "Form1")
            AssertValidationMessageExists("NameValidator")
        End Sub

        Public Sub TestNameMustBeBobValidation()
            _browser.EnterInputValue("Name", "Karl")
            _browser.PushButton("CartoonName", "Form1")
            AssertValidationMessageExists("NameValueValidator")
            AssertLabelText("BuiltName", String.Empty)
        End Sub
    End Class
End Namespace