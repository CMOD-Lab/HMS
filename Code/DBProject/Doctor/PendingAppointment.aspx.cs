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
    public partial class pendingappointment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
                loadgrid();   
        }


        public void loadgrid()
        {
                myDAL objDAL = new myDAL();
                
               // cr-dotnet-0045: Redis-backed distributed session
               int did = (int)SessionHelper.Get("idoriginal");

                    DataTable DT = new DataTable();

                objDAL.GetAllpendingappointments_DAL(did, ref DT);

                pendingappointments.DataSource = DT;
                // cr-dotnet-1034: Async DataBind using RegisterAsyncTask
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await Task.Run(() => pendingappointments.DataBind());
                }));
               
        }


        

        

        protected void update_appointment(Object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {

            if (e.CommandName == "Select")
            {
                // Retrieve the row that raised the event from the Rows

                Int16 num = Convert.ToInt16(e.CommandArgument);

                string aId = pendingappointments.Rows[num].Cells[1].Text;

                //retrieve appointmentid  from that row (key-non editable)
                int appointmentid = Convert.ToInt32(aId);

                //=====updating the newly entered values in database====
                myDAL objmyDAL = new myDAL();
                int result = objmyDAL.UpdateAppointment_DAL(appointmentid);
                //reload the page======================================================
                pendingappointments.EditIndex = -1;
                loadgrid();
            }
        }


        protected void Delete_appointment(Object sender, GridViewDeleteEventArgs e)
        {
            // Retrieve the row that raised the event from the Rows
            // collection of the GridView control.
            GridViewRow row = pendingappointments.Rows[e.RowIndex];

            //get appointmentid from that row
            int appointmentid = Convert.ToInt32(row.Cells[1].Text.ToString());


            //Call the DAL function to delete the student with this roll number 
            myDAL objDAL = new myDAL();

            if (objDAL.Deleteappointment_DAL(appointmentid) == 1)
            {  
                loadgrid(); //reload the grid to show the modifications in table
            }
        }


    }
}
