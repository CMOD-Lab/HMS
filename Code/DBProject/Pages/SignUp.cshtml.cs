using DBProject.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages
{
	public class SignUpModel : PageModel
	{
		public string AlertMessage { get; private set; }

		[BindProperty] public string LoginEmail { get; set; }
		[BindProperty] public string LoginPassword { get; set; }

		[BindProperty] public string SName { get; set; }
		[BindProperty] public string SBirthDate { get; set; }
		[BindProperty] public string SEmail { get; set; }
		[BindProperty] public string SPassword { get; set; }
		[BindProperty] public string ScPassword { get; set; }
		[BindProperty] public string Phone { get; set; }
		[BindProperty] public string Gender { get; set; }
		[BindProperty] public string Address { get; set; }

		public void OnGet()
		{
			// Equivalent of the Web Forms Page_Load: reset the logged-in identity.
			HttpContext.Session.Remove("idoriginal");
		}

		//-----------------------Function1--------------------------//
		public IActionResult OnPostLogin()
		{
			string email = LoginEmail ?? "";
			string password = LoginPassword ?? "";

			myDAL objmyDAl = new myDAL();

			int status = 0;
			int type = 0;
			int id = 0;

			status = objmyDAl.validateLogin(email, password, ref type, ref id);

			if (status == 0)
			{
				HttpContext.Session.SetInt32("idoriginal", id);

				if (type == 1)
				{
					return RedirectToPage("/Patient/PatientHome");
				}
				else if (type == 2)
				{
					return RedirectToPage("/Doctor/DoctorHome");
				}
				else if (type == 3)
				{
					return RedirectToPage("/Admin/AdminHome");
				}
			}
			else if (status == 1)
			{
				AlertMessage = "Email not found. Try Again !";
			}
			else if (status == 2)
			{
				AlertMessage = "Incorrect Password. Try Again !";
			}
			else if (status == -1)
			{
				AlertMessage = "There was some error. Try Again !";
			}

			return Page();
		}

		//-----------------------Function2--------------------------//
		public IActionResult OnPostSignUp()
		{
			string gender = Gender ?? "M";

			myDAL objmyDAl = new myDAL();

			int id = 0;

			int status = objmyDAl.validateUser(SName ?? "", SBirthDate ?? "", SEmail ?? "", SPassword ?? "", Phone ?? "", gender, Address ?? "", ref id);

			//status == 0 failure
			if (status == 0)
			{
				AlertMessage = "Email already exists. Please choose a different one.";
			}
			else if (status == 1)
			{
				HttpContext.Session.SetInt32("idoriginal", id);
				return RedirectToPage("/Patient/PatientHome");
			}
			else if (status == -1)
			{
				AlertMessage = "There was some error. Try again !";
			}

			return Page();
		}
	}
}
