using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Doctor
{
	public class PatientHistoryModel : PageModel
	{
		public DataTable Patients { get; private set; }

		public bool HasError { get; private set; }

		public async Task<IActionResult> OnGetAsync()
		{
			int? did = HttpContext.Session.GetInt32("idoriginal");
			if (!did.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmydal = new myDAL();

			var (found, dt) = await objmydal.SearchPatientAsync(did.Value, HttpContext.RequestAborted);
			if (found != 1)
			{
				HasError = true;
			}
			else
			{
				Patients = dt ?? new DataTable();
			}

			return Page();
		}

		/*SELECT COMMAND: remember the chosen appointment (in the distributed session) and go to the history update page*/
		public IActionResult OnPostSelect(int appointmentId)
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			HttpContext.Session.SetInt32("appointid", appointmentId);
			return Redirect("/Doctor/HistoryUpdate");
		}
	}
}
