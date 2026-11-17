using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class CurrentAppointmentModel : PageModel
	{
		public string Appointment { get; private set; }

		public string DoctorMessage { get; private set; }

		public string TimingsMessage { get; private set; }

		//-----------------------Function1--------------------------//
		public IActionResult OnGet()
		{
			int? sessionPid = HttpContext.Session.GetInt32("idoriginal");
			if (!sessionPid.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAl = new myDAL();

			int pid = sessionPid.Value;

			string dName = "";
			string timings = "";

			int status = objmyDAl.appointmentTodayDisplayer(pid, ref dName, ref timings);

			if (status == -1)
			{
				Appointment = "There was some error in retrieving the Patient's appointment.";
			}
			else if (status == 0)
			{
				Appointment = "You have no appointment today with any doctor.";
			}
			else
			{
				if (status == 3)
				{
					DoctorMessage = "You had an outdated appointment with Doctor " + dName + " to which he didn't respond. So that appointment is discarded.";
					TimingsMessage = "The Appointment Timings were : " + timings;
					return Page();
				}
				else if (status == 2)
				{
					DoctorMessage = "You have sent an appointment request to Doctor " + dName + " which isn't approved by him yet.";
				}
				else
				{
					DoctorMessage = "Today you have an appointment with Doctor " + dName;
				}

				TimingsMessage = "The Appointment Timings are : " + timings;
			}

			return Page();
		}
	}
}
