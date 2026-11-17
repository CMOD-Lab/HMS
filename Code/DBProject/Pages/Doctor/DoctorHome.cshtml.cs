using System.Data;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Doctor
{
	public class DoctorHomeModel : PageModel
	{
		public bool HasError { get; private set; }

		public string Name { get; private set; }
		public string Phone { get; private set; }
		public string Address { get; private set; }
		public string BirthDate { get; private set; }
		public string Gender { get; private set; }
		public string DepartmentNo { get; private set; }
		public string ChargesPerVisit { get; private set; }
		public string MonthlySalary { get; private set; }
		public string ReputeIndex { get; private set; }
		public string PatientsTreated { get; private set; }
		public string Qualification { get; private set; }
		public string Specialization { get; private set; }
		public string WorkExperience { get; private set; }
		public string Status { get; private set; }

		public IActionResult OnGet()
		{
			// Session is backed by a distributed cache (see Program.cs) so it works across instances.
			int? did = HttpContext.Session.GetInt32("idoriginal");
			if (!did.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAL = new myDAL();
			DataTable dt = new DataTable();

			int found = objmyDAL.docinfo_DAL(did.Value, ref dt);

			if (found != 1)
			{
				HasError = true;
			}
			else
			{
				Name = dt.Rows[0][1].ToString();
				Phone = dt.Rows[0][2].ToString();
				Address = dt.Rows[0][3].ToString();
				BirthDate = dt.Rows[0][4].ToString();
				Gender = dt.Rows[0][5].ToString();
				DepartmentNo = dt.Rows[0][6].ToString();
				ChargesPerVisit = dt.Rows[0][7].ToString();
				MonthlySalary = dt.Rows[0][8].ToString();
				ReputeIndex = dt.Rows[0][9].ToString();
				PatientsTreated = dt.Rows[0][10].ToString();
				Qualification = dt.Rows[0][11].ToString();
				Specialization = dt.Rows[0][12].ToString();
				WorkExperience = dt.Rows[0][13].ToString();
				Status = dt.Rows[0][14].ToString();
			}

			return Page();
		}
	}
}
