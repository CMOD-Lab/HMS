using System;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class AppointmentRequestSentModel : PageModel
	{
		public string Message { get; private set; }

		public IActionResult OnGet()
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			return Page();
		}

		//-----------------------Function1--------------------------//
		public IActionResult OnPost()
		{
			int? pIDValue = HttpContext.Session.GetInt32("idoriginal");
			if (!pIDValue.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAl = new myDAL();

			string dID1 = HttpContext.Session.GetString("dID");

			int dID = Convert.ToInt32(dID1);

			int pID = pIDValue.Value;

			string temp = HttpContext.Session.GetString("freeSlot");

			int freeSlot = Convert.ToInt32(temp);

			string mes = "";

			int status = objmyDAl.insertAppointment(dID, pID, freeSlot, ref mes);

			if (status == -1)
			{
				Message = "There was some error in sending appointment request to the Doctor.";
			}
			else
			{
				Message = mes;
			}

			return Page();
		}
	}
}
