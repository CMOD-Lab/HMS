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

namespace DBProject
{
    // Migration note (cr-dotnet-0026): In ASP.NET Core, this class would become a
    // Razor Page PageModel instead of inheriting from System.Web.UI.Page.
    public partial class PatientHome : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            patientInfo(sender,e);
        }

        

 
        //-----------------------Function1--------------------------//

        protected void patientInfo (object sender, EventArgs e)
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0045: Redis-backed distributed session
            int pid = (int)SessionHelper.Get("idoriginal");

            string name      = "";
            string phone     = "";
            string address   = "";
            string birthDate = "";
            int    age       = 0 ;
            string gender    = "";


            int status = objmyDAl.patientInfoDisplayer(pid, ref name, ref phone, ref address, ref birthDate, ref age, ref gender);

            if (status == -1)
            {
                Response.Write("<script>alert('There was some error in retrieving the Patient's Info.');</script>");
            }

            else if (status == 0)
            {
                PName.Text      = name;
                PPhone.Text     = phone;
                PBirthDate.Text = birthDate;
                PatientAge.Text = age.ToString();
                PAddress.Text   = address;
                PGender.Text    = gender;
            }

            return;
        }


        //-----------------------Add a new function here------------------//
    }
}