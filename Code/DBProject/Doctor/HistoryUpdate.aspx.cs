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

namespace doctor
{
    // Migration note (cr-dotnet-0026): In ASP.NET Core, this class would become a
    // Razor Page PageModel instead of inheriting from System.Web.UI.Page.
    public partial class Historyupdate : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void saveindatabase(object sender, EventArgs e)
        {
            myDAL objmyDAL = new myDAL();
            int found;
            // cr-dotnet-0045: Redis-backed distributed session
            int did = (int)SessionHelper.Get("idoriginal");
            string disease= Disease.Text;
            string progres = progress.Text;
            string prescrip = Prescription.Text;

            // cr-dotnet-0045: Redis-backed distributed session
            int appid = (int)SessionHelper.Get("appointid");

            
            found = objmyDAL.update_prescription_DAL(did,appid,disease,progres,prescrip);

            if (found != 1)
            { Response.Write("<script>alert('There was some error');</script>"); }
            else
            {
                { Response.Write("<script>alert('Information Successfully Updated');</script>"); }
            }
        }


        public void generate_bill(object sender, EventArgs e)
        {
            // cr-dotnet-0126: Stateless redirect using Redis cache instead of sticky sessions
            SessionHelper.Set("RedirectTarget", "bill.aspx");
            Response.Redirect("bill.aspx");
        }
    }
}