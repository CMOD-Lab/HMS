using System;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class DoctorProfileModel : PageModel
	{
		public string ErrorMessage { get; private set; }

		public string DName { get; private set; }
		public string DPhone { get; private set; }
		public string DQualification { get; private set; }
		public string DSpecialization { get; private set; }
		public string DWork { get; private set; }
		public string DAge { get; private set; }
		public string DGender { get; private set; }
		public string DDept { get; private set; }
		public string DCharges { get; private set; }
		public string DRI { get; private set; }
		public string DPT { get; private set; }

		//-----------------------Function1--------------------------//
		public IActionResult OnGet()
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAl = new myDAL();

			string dID1 = HttpContext.Session.GetString("dID");

			int dID = Convert.ToInt32(dID1);

			string name = "";
			string phone = "";
			string gender = "";

			float charges_Per_Visit = 0;
			float ReputeIndex = 0;
			int PatientsTreated = 0;
			string qualification = "";
			string specialization = "";
			int workE = 0;
			int age = 0;

			string deptName = HttpContext.Session.GetString("deptOriginal");

			int status = objmyDAl.doctorInfoDisplayer(dID, ref name, ref phone, ref gender, ref charges_Per_Visit, ref ReputeIndex, ref PatientsTreated, ref qualification, ref specialization, ref workE, ref age);

			if (status == -1)
			{
				ErrorMessage = "There was some error in retrieving the Doctor's Info.";
			}
			else if (status == 0)
			{
				DName = name;
				DPhone = phone;
				DQualification = qualification;
				DSpecialization = specialization;
				DWork = workE.ToString();
				DAge = age.ToString();
				DGender = gender;
				DDept = deptName;
				DCharges = charges_Per_Visit.ToString();
				DRI = ReputeIndex.ToString();
				DPT = PatientsTreated.ToString();
			}

			return Page();
		}

		//-----------------------Function2------------------//
		public IActionResult OnPost()
		{
			return RedirectToPage("/Patient/AppointmentTaker");
		}
	}
}
