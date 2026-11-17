using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Doctor
{
	public class PendingAppointmentModel : PageModel
	{
		public DataTable Appointments { get; private set; }

		public async Task<IActionResult> OnGetAsync()
		{
			int? did = HttpContext.Session.GetInt32("idoriginal");
			if (!did.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			await LoadGridAsync(did.Value);
			return Page();
		}

		/*THIS HANDLER IS CALLED WHEN THE DOCTOR APPROVES THE REQUEST OF A PATIENT*/
		public async Task<IActionResult> OnPostApproveAsync(int appointmentId)
		{
			int? did = HttpContext.Session.GetInt32("idoriginal");
			if (!did.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAL = new myDAL();
			await objmyDAL.UpdateAppointmentAsync(appointmentId, HttpContext.RequestAborted);

			//reload the page
			await LoadGridAsync(did.Value);
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(int appointmentId)
		{
			int? did = HttpContext.Session.GetInt32("idoriginal");
			if (!did.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objDAL = new myDAL();
			await objDAL.DeleteAppointmentAsync(appointmentId, HttpContext.RequestAborted);

			//reload the grid to show the modifications in table
			await LoadGridAsync(did.Value);
			return Page();
		}

		private async Task LoadGridAsync(int did)
		{
			myDAL objDAL = new myDAL();

			Appointments = await objDAL.GetAllPendingAppointmentsAsync(did, HttpContext.RequestAborted) ?? new DataTable();
		}
	}
}
