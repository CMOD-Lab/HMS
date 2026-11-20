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


namespace doctor
{
    // Migration note (cr-dotnet-0026): In ASP.NET Core, this class would become a
    // Razor Page PageModel instead of inheriting from System.Web.UI.Page.
    public partial class patienthistory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
                myDAL objmydal = new myDAL();
                DataTable dt = new DataTable();
                int found = 0;

                // cr-dotnet-0045: Redis-backed distributed session
                int did = (int)SessionHelper.Get("idoriginal");

                found = objmydal.search_patient_DAL(did, ref dt);
                if (found != 1)
                { Response.Write("<script>alert('There was some error');</script>"); }
                else
                {
                    patientsgrid.DataSource = dt;
                    // cr-dotnet-1034: Async DataBind using RegisterAsyncTask
                    RegisterAsyncTask(new PageAsyncTask(async () =>
                    {
                        await Task.Run(() => patientsgrid.DataBind());
                    }));
                }
        }

        
        protected void patientsgrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {

                Int16 num = Convert.ToInt16(e.CommandArgument);

                string aId = patientsgrid.Rows[num].Cells[1].Text;

                //retrieve appointmentid  from that row (key-non editable)
                int appointmentid = Convert.ToInt32(aId);

                // cr-dotnet-0045: Redis-backed distributed session
                // cr-dotnet-0126: Stateless session using Redis cache instead of sticky sessions
                SessionHelper.Set("appointid", appointmentid);
                // cr-dotnet-0126: Stateless redirect using Redis cache instead of sticky sessions
                SessionHelper.Set("RedirectTarget", "Historyupdate.aspx");
                Response.Redirect("Historyupdate.aspx");
            }
        }
    }

}