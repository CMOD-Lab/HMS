using System;
using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class ViewDoctorsModel : PageModel
	{
		public string Message { get; private set; }

		public DataTable Doctors { get; private set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			HttpContext.Session.SetString("dID", "");
			await DeptDoctorInfoAsync();
			return Page();
		}

		//---------------Handler called whenever a Doctor is selected from the table----//
		public async Task<IActionResult> OnPostSelectAsync(int row)
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			await DeptDoctorInfoAsync();

			if (Doctors != null && row >= 0 && row < Doctors.Rows.Count)
			{
				string dID = Convert.ToString(Doctors.Rows[row][0]);

				HttpContext.Session.SetString("dID", dID);

				return RedirectToPage("/Patient/DoctorProfile");
			}

			return Page();
		}

		//-----------------------Function1--------------------------//
		private async Task DeptDoctorInfoAsync()
		{
			myDAL objmyDAl = new myDAL();

			string deptName = HttpContext.Session.GetString("deptOriginal");

			(int status, DataTable DT) = await objmyDAl.GetDeptDoctorInfoAsync(deptName, HttpContext.RequestAborted);

			if (status == -1)
			{
				Message = "There was some error in retrieving the Doctors Information.";
			}
			else
			{
				Message = "Following are our Specialized Doctors of " + deptName + " Department:";
				Doctors = DT;
			}
		}
	}
}
