using System.Data;
using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Doctor
{
	public class BillModel : PageModel
	{
		public string BillAmount { get; private set; }

		public bool HasError { get; private set; }

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

			int found = objmyDAL.generate_bill_DAL(did.Value, ref dt);

			if (found != 1)
			{
				HasError = true;
			}
			else
			{
				BillAmount = dt.Rows[0][0].ToString();
			}

			return Page();
		}

		public IActionResult OnPostPaid()
		{
			int? did = HttpContext.Session.GetInt32("idoriginal");
			int? appoint = HttpContext.Session.GetInt32("appointid");
			if (!did.HasValue || !appoint.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAL = new myDAL();
			objmyDAL.paid_bill_DAL(did.Value, appoint.Value);

			return Redirect("/Doctor/PatientHistory");
		}

		public IActionResult OnPostUnpaid()
		{
			int? did = HttpContext.Session.GetInt32("idoriginal");
			int? appoint = HttpContext.Session.GetInt32("appointid");
			if (!did.HasValue || !appoint.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAL = new myDAL();
			objmyDAL.Unpaid_bill_DAL(did.Value, appoint.Value);

			return Redirect("/Doctor/PatientHistory");
		}
	}
}
