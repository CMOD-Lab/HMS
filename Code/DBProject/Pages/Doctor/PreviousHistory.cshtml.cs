using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Doctor
{
	public class PreviousHistoryModel : PageModel
	{
		public DataTable History { get; private set; }

		public string ErrorMessage { get; private set; }

		public async Task<IActionResult> OnGetAsync()
		{
			int? id = HttpContext.Session.GetInt32("idoriginal");
			if (!id.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAl = new myDAL();

			var (status, DT) = await objmyDAl.GetPHistoryAsync(id.Value, HttpContext.RequestAborted);

			if (status == -1)
			{
				ErrorMessage = "There was some error in retrieving the Patients History.";
			}
			else
			{
				History = DT;
			}

			return Page();
		}
	}
}
