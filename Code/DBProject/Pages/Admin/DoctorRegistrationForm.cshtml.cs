using System.ComponentModel.DataAnnotations;
using DBProject.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Admin
{
	public class DoctorRegistrationFormModel : PageModel
	{
		private const string EmailPattern = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])";

		[BindProperty]
		[Required(ErrorMessage = "* Required")]
		public string Name { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		[RegularExpression(@"((?:0[1-9])|(?:1[0-2]))\/((?:0[0-9])|(?:[1-2][0-9])|(?:3[0-1]))\/(\d{4})", ErrorMessage = "Birth Date Format Not Correct")]
		public string BirthDate { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		[RegularExpression(EmailPattern, ErrorMessage = "Incorrect Email Format")]
		public string Email { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		[Compare(nameof(CPassword), ErrorMessage = "Passwords Do not Match ")]
		public string Password { get; set; }

		[BindProperty]
		public string CPassword { get; set; }

		[BindProperty]
		[RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
		public string Phone { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		[RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
		public string Salary { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		[RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
		public string Charges_per_visit { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		[Range(0, 5, ErrorMessage = "Experience Range should be (0-5)")]
		public int? Exp { get; set; }

		[BindProperty]
		public int Department { get; set; }

		[BindProperty]
		public string Qualification { get; set; }

		[BindProperty]
		public string Spec { get; set; }

		[BindProperty]
		public string Address { get; set; }

		[BindProperty]
		public string Gender { get; set; } = "M";

		public string Message { get; private set; }

		public void OnGet()
		{
		}

		public IActionResult OnPost()
		{
			ValidateDoctorEmail();
			DepartmentValidate();

			if (!ModelState.IsValid)
			{
				return Page();
			}

			if (!int.TryParse(Salary, out int salary))
			{
				ModelState.AddModelError(nameof(Salary), "Numbers Only !");
				return Page();
			}
			if (!int.TryParse(Charges_per_visit, out int chargesPerVisit))
			{
				ModelState.AddModelError(nameof(Charges_per_visit), "Numbers Only !");
				return Page();
			}

			myDAL objmyDAL = new myDAL();

			int exp = Exp.Value;
			int dept = Department;
			string gender = string.IsNullOrEmpty(Gender) ? "M" : Gender;

			objmyDAL.AddDoctor(Name, Email, Password, BirthDate, dept, Phone, gender[0], Address, exp, salary, chargesPerVisit, Spec, Qualification);
			Message = "doctor Added Succesfully";
			flushInformation();

			return Page();
		}

		/*SERVER-SIDE CHECK THAT THE DOCTOR EMAIL IS NOT ALREADY REGISTERED*/
		protected void ValidateDoctorEmail()
		{
			if (string.IsNullOrEmpty(Email) || ModelState[nameof(Email)]?.Errors.Count > 0)
			{
				return;
			}

			myDAL objmyDAL = new myDAL();

			if (objmyDAL.DoctorEmailAlreadyExist(Email) == 1)
			{
				Message = null;
				ModelState.AddModelError(nameof(Email), "This Email Already exist , kindly choose a different one !");
			}
		}

		protected void DepartmentValidate()
		{
			if (Department == 0)
			{
				ModelState.AddModelError(nameof(Department), "Please Select Department");
			}
		}

		protected void flushInformation()
		{
			ModelState.Clear();
			Name = "";
			Email = "";
			Password = "";
			CPassword = "";
			BirthDate = "";
			Department = 0;
			Phone = "";
			Address = "";
			Exp = null;
			Salary = "";
			Charges_per_visit = "";
			Spec = "";
			Qualification = "";
		}
	}
}
