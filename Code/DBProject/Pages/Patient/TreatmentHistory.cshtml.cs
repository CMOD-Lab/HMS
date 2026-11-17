using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class TreatmentHistoryModel : PageModel
	{
		public string Message { get; private set; }

		public DataTable History { get; private set; }

		//-----------------------Function1--------------------------//
		public async Task<IActionResult> OnGetAsync()
		{
			int? id = HttpContext.Session.GetInt32("idoriginal");
			if (!id.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAl = new myDAL();

			(int status, DataTable DT) = await objmyDAl.GetTreatmentHistoryAsync(id.Value, HttpContext.RequestAborted);

			if (status == -1)
			{
				Message = "There was some error in retrieving the Patient's Treatment History.";
			}
			else if (status == 0)
			{
				Message = "There is currently no treatment history of yours.";
			}
			else
			{
				Message = "Treatment History of " + status + " Appointment(s) is found: ";
				History = DT;
			}

			return Page();
		}
	}
}
