using System;
using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class AppointmentTakerModel : PageModel
	{
		public string Message { get; private set; }

		public DataTable Slots { get; private set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			HttpContext.Session.SetString("freeSlot", "");
			await FreeSlotsAsync();
			return Page();
		}

		//---------------Handler called whenever a Free Slot is selected from the grid----//
		public async Task<IActionResult> OnPostSelectAsync(int row)
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			await FreeSlotsAsync();

			if (Slots != null && row >= 0 && row < Slots.Rows.Count)
			{
				string appointment = Convert.ToString(Slots.Rows[row][0]);

				string[] tokens = appointment.Split(':');

				HttpContext.Session.SetString("freeSlot", tokens[0]);

				return RedirectToPage("/Patient/AppointmentRequestSent");
			}

			return Page();
		}

		//-----------------------Function1--------------------------//
		private async Task FreeSlotsAsync()
		{
			myDAL objmyDAl = new myDAL();

			string dID1 = HttpContext.Session.GetString("dID");

			int dID = Convert.ToInt32(dID1);

			int pID = HttpContext.Session.GetInt32("idoriginal").Value;

			var (status, DT) = await objmyDAl.GetFreeSlotsAsync(dID, pID, HttpContext.RequestAborted);

			if (status == -1)
			{
				Message = "There was some error in retrieving the Doctors's Free Slots.";
			}
			else if (status == 0)
			{
				Message = "There is currently no free slot of this doctor.";
			}
			else if (status > 0)
			{
				Message = "The following are the " + status + " free slots of this doctor for today :";
				Slots = DT;
			}
		}
	}
}
