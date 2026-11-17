using System;
using System.ComponentModel.DataAnnotations;
using DBProject.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Admin
{
	public class AddStaffModel : PageModel
	{
		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		public string Name { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		[RegularExpression(@"((?:0[1-9])|(?:1[0-2]))\/((?:0[0-9])|(?:[1-2][0-9])|(?:3[0-1]))\/(\d{4})", ErrorMessage = "Birth Date Format Not Correct")]
		public string BirthDate { get; set; }

		[BindProperty]
		[RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
		public string Phone { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		[RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
		public string Salary { get; set; }

		[BindProperty]
		public string Qual { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "*Required")]
		public string Designation { get; set; }

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
			if (!ModelState.IsValid)
			{
				return Page();
			}

			if (!int.TryParse(Salary, out int salary))
			{
				ModelState.AddModelError(nameof(Salary), "Numbers Only !");
				return Page();
			}

			myDAL objmyDAL = new myDAL();
			string gender = string.IsNullOrEmpty(Gender) ? "M" : Gender;

			if (objmyDAL.AddStaff(Name, BirthDate, Phone, gender[0], Address, salary, Qual, Designation) == 1)
			{
				Message = Designation + " Added Succesfully";
				flushInformation();
			}
			else
			{
				Message = "There was some error";
			}

			return Page();
		}

		protected void flushInformation()
		{
			ModelState.Clear();
			Name = "";
			BirthDate = "";
			Phone = "";
			Address = "";
			Salary = "";
			Qual = "";
			Designation = "";
		}
	}
}
