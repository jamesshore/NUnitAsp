using System;
using System.Data;

namespace NUnitAspTestPages
{
	public class RedirectionTarget : System.Web.UI.Page
	{
		protected System.Web.UI.WebControls.DataGrid formVars;
	
		private void Page_Load(object sender, System.EventArgs e)
		{
			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn("Name", typeof(string)));
			table.Columns.Add(new DataColumn("Value", typeof(string)));

			foreach (string name in Request.Form)
			{
				DataRow row = table.NewRow();
				row["Name"] = name;
				row["Value"] = Request.Form[name];
				table.Rows.Add(row);
			}

			formVars.DataSource = table;
			formVars.DataBind();
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion
	}
}
