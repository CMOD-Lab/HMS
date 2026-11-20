using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
// Migration note (cr-dotnet-0026): System.Web.UI and System.Web.UI.WebControls are
// Web Forms namespaces. In the ASP.NET Core migration, these will be replaced by
// Microsoft.AspNetCore.Mvc.RazorPages / Microsoft.AspNetCore.Mvc.
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;

namespace DBProject
{
	// Migration note (cr-dotnet-0026): In ASP.NET Core, this class would become a
	// Razor Page PageModel instead of inheriting from System.Web.UI.Page.
	public partial class AdminHome : System.Web.UI.Page
	{

		protected void Page_Load(object sender, EventArgs e)
		{
			GetAdminHomeInformation();
		}



		public void GetAdminHomeInformation()
		{
			myDAL objmyDAL = new myDAL();

			DataTable[] arrTable = new DataTable[5];
			for (int i = 0; i < 5; i++)
			{
				arrTable[i] = new DataTable();
			}


			objmyDAL.GetAdminHomeInformation(ref arrTable);

			Total_Doctors.Visible = TotalIncome.Visible = TotalPatients.Visible = true;

			Total_Doctors.Text = arrTable[0].Rows[0][0].ToString();
			TotalPatients.Text = arrTable[1].Rows[0][0].ToString();
			TotalIncome.Text = arrTable[2].Rows[0][0].ToString();



			department_View.DataSource = arrTable[3];
            // cr-dotnet-1034: Async DataBind using RegisterAsyncTask
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                await Task.Run(() => department_View.DataBind());
            }));


            Appointment_view.DataSource = arrTable[4];
            // cr-dotnet-1034: Async DataBind using RegisterAsyncTask
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                await Task.Run(() => Appointment_view.DataBind());
            }));
			







		}

	}

}