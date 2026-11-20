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
    public partial class BillsHistory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            billHistory(sender, e);
        }


        //-----------------------Function1--------------------------//

        protected void billHistory(object sender, EventArgs e)
        {
            myDAL objmyDAl = new myDAL();

            DataTable DT = new DataTable();


            // cr-dotnet-0045: Redis-backed distributed session
            int id = (int)SessionHelper.Get("idoriginal");


            int status = objmyDAl.getBillHistory(id, ref DT);


            if (status == -1)
            {
                BHistory.Text = "There was some error in retrieving the Patient's Bill History.";
            }

            else if (status == 0)
            {
                BHistory.Text = "There is currently no bill history of yours.";
            }

            else
            {
                BHistory.Text = status + " Bill(s) are found: ";
                BHistoryGrid.DataSource = DT;
                // cr-dotnet-1034: Async DataBind using RegisterAsyncTask
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    await Task.Run(() => BHistoryGrid.DataBind());
                }));
            }

            return;
        }


        //-----------------------Add a new function here------------------//


    }

}