using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class PatientNotificationsModel : PageModel
	{
		public string Notify { get; private set; }

		public string NDoctor { get; private set; }

		public string NTimings { get; private set; }

		public IActionResult OnGet()
		{
			int? sessionPid = HttpContext.Session.GetInt32("idoriginal");
			if (!sessionPid.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			Notifications(sessionPid.Value);
			return Page();
		}

		//-----------------------Function1--------------------------//
		private void Notifications(int pid)
		{
			myDAL objmyDAl = new myDAL();

			string dName = "";
			string timings = "";

			int status = objmyDAl.getNotifications(pid, ref dName, ref timings);

			if (status == -1)
			{
				Notify = "There was some error in retrieving the Patient's notifications.";
			}
			else if (status == 0)
			{
				Notify = "There are no new notifications :)";
			}
			else if (status == 1)
			{
				NDoctor = "Your requested appointment with Doctor " + dName + " has been accepted by him! :)";
				NTimings = "The Appointment Timings are : " + timings;
			}
			else if (status == 2)
			{
				NDoctor = "Your requested appointment with Doctor " + dName + " has been rejected by him! :(";
				NTimings = "The Appointment Timings were : " + timings;
			}
			else if (status == 3)
			{
				NDoctor = "Your appointment with Doctor " + dName + " has been completed now. We hope you are feeling better now!";
				NTimings = "The Appointment Timings were : " + timings;
			}
		}
	}
}
