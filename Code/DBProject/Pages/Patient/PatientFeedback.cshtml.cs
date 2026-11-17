using System;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class PatientFeedbackModel : PageModel
	{
		public string FeedbackMessage { get; private set; }

		[BindProperty]
		public string FDoctor { get; set; }

		[BindProperty]
		public string FTimings { get; set; }

		[BindProperty]
		public int Rating { get; set; } = 1;

		public bool ShowForm { get; private set; }

		public string ResultMessage { get; private set; }

		public IActionResult OnGet()
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			HttpContext.Session.Remove("aID");
			PendingFeedback();
			return Page();
		}

		//-----------------------Function1--------------------------//
		private void PendingFeedback()
		{
			myDAL objmyDAl = new myDAL();

			int pid = HttpContext.Session.GetInt32("idoriginal").Value;

			string dName = "";
			string timings = "";

			int aID = 0;

			int status = objmyDAl.isFeedbackPending(pid, ref dName, ref timings, ref aID);

			if (status == -1)
			{
				FeedbackMessage = "There was some error in retrieving the Pending Feedbacks.";
			}
			else if (status == 0)
			{
				FeedbackMessage = "There are no pending feedbacks :)";
			}
			else
			{
				HttpContext.Session.SetInt32("aID", aID);

				FDoctor = "Your feedback for the appointment with Doctor " + dName + " is pending. Kindly give it.";
				FTimings = "The Appointment Timings were : " + timings;

				//Show the feedback form
				ShowForm = true;
			}
		}

		//-----------------------Function2--------------------------//
		public IActionResult OnPost()
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAl = new myDAL();

			int? aID = HttpContext.Session.GetInt32("aID");

			ShowForm = true;

			if (!aID.HasValue)
			{
				ResultMessage = "There was some error.";
				return Page();
			}

			int status = objmyDAl.givePendingFeedback(aID.Value);

			if (status == -1)
			{
				ResultMessage = "There was some error.";
			}
			else if (status == 0)
			{
				ResultMessage = "Thank you for your feedback :)";
			}

			return Page();
		}
	}
}
