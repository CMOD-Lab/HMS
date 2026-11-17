using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class BillsHistoryModel : PageModel
	{
		public string Message { get; private set; }

		public DataTable History { get; private set; }

		//-----------------------Function1--------------------------//
		public async Task<IActionResult> OnGetAsync()
		{
			int? pid = HttpContext.Session.GetInt32("idoriginal");
			if (!pid.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAl = new myDAL();

			var (status, DT) = await objmyDAl.GetBillHistoryAsync(pid.Value, HttpContext.RequestAborted);

			if (status == -1)
			{
				Message = "There was some error in retrieving the Patient's Bill History.";
			}
			else if (status == 0)
			{
				Message = "There is currently no bill history of yours.";
			}
			else
			{
				Message = status + " Bill(s) are found: ";
				History = DT;
			}

			return Page();
		}
	}
}
