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
    public partial class AppointmentTaker : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // cr-dotnet-0045: Redis-backed distributed session
            SessionHelper.Set("freeSlot", "");
            freeSlots(sender, e);
        }


        //---------------Function Called whenever a Free Slot is selected from the Grid View----//
        protected void PAppointmentGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                Int16 num = Convert.ToInt16(e.CommandArgument);

                string appointment = PAppointmentGrid.Rows[num].Cells[2].Text;

                string[] tokens = appointment.Split(':');

                // cr-dotnet-0045: Redis-backed distributed session
                // cr-dotnet-0126: Stateless session using Redis cache instead of sticky sessions
                SessionHelper.Set("freeSlot", tokens[0]);

                // cr-dotnet-0126: Stateless redirect using Redis cache instead of sticky sessions
                SessionHelper.Set("RedirectTarget", "AppointmentRequestSent.aspx");
                Response.BufferOutput = true;
                Response.Redirect("AppointmentRequestSent.aspx");

                return;
            }
        }


        //-----------------------Function1--------------------------//

        protected void freeSlots(object sender, EventArgs e)
        {
            myDAL objmyDAl = new myDAL();

            DataTable DT = new DataTable();


            // cr-dotnet-0045: Redis-backed distributed session
            string dID1 = (string)SessionHelper.Get("dID");

            int dID = Convert.ToInt32(dID1);


            // cr-dotnet-0045: Redis-backed distributed session
            int pID = (int)SessionHelper.Get("idoriginal");

            
            int status = objmyDAl.getFreeSlots(dID, pID, ref DT);


            if (status == -1)
            {
                PAppointment.Text = "There was some error in retrieving the Doctors's Free Slots.";
            }

            else if (status == 0)
            {
                PAppointment.Text = "There is currently no free slot of this doctor.";
            }

            else if (status > 0)
            {
                PAppointment.Text = "The following are the " + status  + " free slots of this doctor for today :";
                PAppointmentGrid.DataSource = DT;
                // cr-dotnet-1034: Async DataBind using RegisterAsyncTask
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await Task.Run(() => PAppointmentGrid.DataBind());
                }));
            }

            return;
        }


        //-----------------------Add a new function here------------------//

    }
}