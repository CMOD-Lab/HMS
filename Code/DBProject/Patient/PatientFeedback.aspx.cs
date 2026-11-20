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
    public partial class PatientFeedback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
			if (!IsPostBack)
			{
				// cr-dotnet-0045: Redis-backed distributed session
			SessionHelper.Set("aID", "");
				pendingFeedback(sender, e);
			}
        }



        
        //-----------------------Function1--------------------------//

        protected void pendingFeedback(object sender, EventArgs e)
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0045: Redis-backed distributed session
            int pid = (int)SessionHelper.Get("idoriginal");

            string dName = "";
            string timings = "";

            int aID = 0;

            int status = objmyDAl.isFeedbackPending(pid, ref dName, ref timings, ref aID);

            if (status == -1)
            {
                Feedback.Text = "There was some error in retrieving the Pending Feedbacks.";
            }

            else if (status == 0)
            {
                Feedback.Text = "There are no pending feedbacks :)";
            }

            else
            {
                // cr-dotnet-0045: Redis-backed distributed session
                // cr-dotnet-0126: Stateless session using Redis cache instead of sticky sessions
                SessionHelper.Set("aID", aID);

                FDoctor.Text = "Your feedback for the appointment with Doctor " + dName + " is pending. Kindly give it.";
                FTimings.Text = "The Appointment Timings were : " + timings;

                //Make the things visible -- Magic O.O --
                Message.Visible = true;
                List.Visible = true;
                button1.Visible = true;

                return;
            }
        }




        //-----------------------Function2--------------------------//

        protected void giveFeedback(object sender, EventArgs e)
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0045: Redis-backed distributed session
            int aID = (int)SessionHelper.Get("aID");


            int rating = Convert.ToInt32(List.SelectedItem.Value);


            int status = objmyDAl.givePendingFeedback(aID);

            if (status == -1)
            {
                F.Text = "There was some error.";
            }

            else if (status == 0)
            {
                F.Text = "Thank you for your feedback :)";
            }

            return;
        }



        //-----------------------Add a new function here------------------//



    }
}