using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Doctor
{
	public class HistoryUpdateModel : PageModel
	{
		[BindProperty]
		public string Disease { get; set; }

		[BindProperty]
		public string Progress { get; set; }

		[BindProperty]
		public string Prescription { get; set; }

		public string Message { get; private set; }

		public IActionResult OnGet()
		{
			if (!HttpContext.Session.GetInt32("idoriginal").HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			return Page();
		}

		public IActionResult OnPostSave()
		{
			int? did = HttpContext.Session.GetInt32("idoriginal");
			int? appid = HttpContext.Session.GetInt32("appointid");
			if (!did.HasValue || !appid.HasValue)
			{
				return Redirect("/SignUp.aspx");
			}

			myDAL objmyDAL = new myDAL();

			int found = objmyDAL.update_prescription_DAL(did.Value, appid.Value, Disease ?? string.Empty, Progress ?? string.Empty, Prescription ?? string.Empty);

			Message = found != 1 ? "There was some error" : "Information Successfully Updated";

			return Page();
		}

		public IActionResult OnPostBill()
		{
			return Redirect("/Doctor/Bill");
		}
	}
}
