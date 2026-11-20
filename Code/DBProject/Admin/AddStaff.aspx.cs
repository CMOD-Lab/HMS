using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
// Migration note (cr-dotnet-0026): System.Web.UI and System.Web.UI.WebControls are
// Web Forms namespaces. In the ASP.NET Core migration, these will be replaced by
// Microsoft.AspNetCore.Mvc.RazorPages / Microsoft.AspNetCore.Mvc.
using System.Web.UI;
using System.Web.UI.WebControls;
using DBProject.DAL;

namespace DBProject
{
	// Migration note (cr-dotnet-0026): In ASP.NET Core, this class would become a
	// Razor Page PageModel instead of inheriting from System.Web.UI.Page.
	public partial class AddStaff : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected void StaffRegister(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				myDAL objmyDAL = new myDAL();

				int salary = Convert.ToInt32(Salary.Text);
				string gender = Request.Form["Gender"].ToString();

				if (objmyDAL.AddStaff(Name.Text, BirthDate.Text, Phone.Text, gender[0], Address.Text, salary, Qual.Text, Designation.Text) == 1) ;
				{
					Response.BufferOutput = true;
					Msg.Visible = true;
					Msg.Text = Designation.Text + " Added Succesfully";
					flushInformation();
				}

			}
		}
		protected void flushInformation()
		{
			Name.Text = "";
			BirthDate.Text = "";
			Phone.Text = "";
			Address.Text = "";
			Salary.Text = "";
			Qual.Text = "";
			Designation.Text = "";
		}

		
	}
}