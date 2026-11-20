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
    public partial class bill : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            myDAL objmyDAL = new myDAL();
            DataTable dt = new DataTable();
            int found;

            // cr-dotnet-0045: Redis-backed distributed session
            int did = (int)SessionHelper.Get("idoriginal");
            
            found = objmyDAL.generate_bill_DAL(did, ref dt);

            if (found != 1)
            { Response.Write("<script>alert('There was some error');</script>"); }
            else
            {
                Label1.Text = dt.Rows[0][0].ToString();
            }
        }


        public void bill_paid(object sender, EventArgs e)
        {
            myDAL objmyDAL = new myDAL();
            
            // cr-dotnet-0045: Redis-backed distributed session
            int did = (int)SessionHelper.Get("idoriginal");
            // cr-dotnet-0045: Redis-backed distributed session
            int appoint = (int)SessionHelper.Get("appointid");
            objmyDAL.paid_bill_DAL(did,appoint);

			Response.BufferOutput = false;
            // cr-dotnet-0126: Stateless redirect using Redis cache instead of sticky sessions
            SessionHelper.Set("RedirectTarget", "patienthistory.aspx");
            Response.Redirect("patienthistory.aspx");
        }


        public void bill_Unpaid(object sender, EventArgs e)
        {
            myDAL objmyDAL = new myDAL();

            // cr-dotnet-0045: Redis-backed distributed session
            int did = (int)SessionHelper.Get("idoriginal");
            // cr-dotnet-0045: Redis-backed distributed session
            int appoint = (int)SessionHelper.Get("appointid");
            objmyDAL.Unpaid_bill_DAL(did, appoint);

            Response.BufferOutput = false;
            // cr-dotnet-0126: Stateless redirect using Redis cache instead of sticky sessions
            SessionHelper.Set("RedirectTarget", "patienthistory.aspx");
            Response.Redirect("patienthistory.aspx");
        }
    }
}