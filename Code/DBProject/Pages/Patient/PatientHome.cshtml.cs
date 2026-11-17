using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class PatientHomeModel : PageModel
	{
		public string ErrorMessage { get; private set; }

		public string PName { get; private set; }
		public string PPhone { get; private set; }
		public string PBirthDate { get; private set; }
		public string PatientAge { get; private set; }
		public string PGender { get; private set; }
		public string PAddress { get; private set; }

		public IActionResult OnGet()
		{
			int? sessionPid = HttpContext.Session.GetInt32("idoriginal");
			if (!sessionPid.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			PatientInfo(sessionPid.Value);
			return Page();
		}

		//-----------------------Function1--------------------------//
		private void PatientInfo(int pid)
		{
			myDAL objmyDAl = new myDAL();

			string name = "";
			string phone = "";
			string address = "";
			string birthDate = "";
			int age = 0;
			string gender = "";

			int status = objmyDAl.patientInfoDisplayer(pid, ref name, ref phone, ref address, ref birthDate, ref age, ref gender);

			if (status == -1)
			{
				ErrorMessage = "There was some error in retrieving the Patient's Info.";
			}
			else if (status == 0)
			{
				PName = name;
				PPhone = phone;
				PBirthDate = birthDate;
				PatientAge = age.ToString();
				PAddress = address;
				PGender = gender;
			}
		}
	}
}
