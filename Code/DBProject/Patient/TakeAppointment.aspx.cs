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
using System.Data;
using System.Threading.Tasks;


namespace DBProject
{
    // Migration note (cr-dotnet-0026): In ASP.NET Core, this class would become a
    // Razor Page PageModel instead of inheriting from System.Web.UI.Page.
    public partial class TakeAppointment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // cr-dotnet-0045: Redis-backed distributed session
            SessionHelper.Set("deptOriginal", "");
            deptInfo(sender, e);

        }

        //---------------Function Called whenever a Department is selected from the Grid View----//
        protected void TDeptGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                Int16 num = Convert.ToInt16(e.CommandArgument);

                string deptName = TDeptGrid.Rows[num].Cells[2].Text;

                // cr-dotnet-0045: Redis-backed distributed session
                // cr-dotnet-0126: Stateless session using Redis cache instead of sticky sessions
                SessionHelper.Set("deptOriginal", deptName);

                // cr-dotnet-0126: Stateless redirect using Redis cache instead of sticky sessions
                SessionHelper.Set("RedirectTarget", "ViewDoctors.aspx");
                Response.BufferOutput = true;
                Response.Redirect("ViewDoctors.aspx");

                return;
            }
        }



        //-----------------------Function1--------------------------//

        protected void deptInfo(object sender, EventArgs e)
        {
            myDAL objmyDAl = new myDAL();

            DataTable DT = new DataTable();


            int status = objmyDAl.getdeptInfo(ref DT);


            if (status == -1)
            {
                TDept.Text = "There was some error in retrieving the Departments Information.";
            }

            else
            {
                TDept.Text = "Following are the departments available at our Clinic : ";
                TDeptGrid.DataSource = DT;
                // cr-dotnet-1034: Async DataBind using RegisterAsyncTask
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await Task.Run(() => TDeptGrid.DataBind());
                }));
            }

            return;
        }


        //-----------------------Add a new function here------------------//





    }
}