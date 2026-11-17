using System;
using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Patient
{
	public class TakeAppointmentModel : PageModel
	{
		public string Message { get; private set; }

		public DataTable Departments { get; private set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			HttpContext.Session.SetString("deptOriginal", "");
			await DeptInfoAsync();
			return Page();
		}

		//---------------Handler called whenever a Department is selected from the table----//
		public async Task<IActionResult> OnPostSelectAsync(int row)
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			await DeptInfoAsync();

			if (Departments != null && row >= 0 && row < Departments.Rows.Count)
			{
				string deptName = Convert.ToString(Departments.Rows[row][0]);

				HttpContext.Session.SetString("deptOriginal", deptName);

				return RedirectToPage("/Patient/ViewDoctors");
			}

			return Page();
		}

		//-----------------------Function1--------------------------//
		private async Task DeptInfoAsync()
		{
			myDAL objmyDAl = new myDAL();

			(int status, DataTable DT) = await objmyDAl.GetDeptInfoAsync(HttpContext.RequestAborted);

			if (status == -1)
			{
				Message = "There was some error in retrieving the Departments Information.";
			}
			else
			{
				Message = "Following are the departments available at our Clinic : ";
				Departments = DT;
			}
		}
	}
}
