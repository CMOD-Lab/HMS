using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Admin
{
	public class ManageClinicModel : PageModel
	{
		/// <summary>D = Doctor, P = Patient, O = Other Staff</summary>
		[BindProperty(SupportsGet = true)]
		public string Category { get; set; } = "D";

		[BindProperty(SupportsGet = true)]
		public string SearchText { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? SelectedId { get; set; }

		public string Msg { get; private set; }

		public DataTable Manage { get; private set; }

		public string Caption { get; private set; } = "Doctors Table";

		/// <summary>Label/value pairs of the selected record (rendered HTML-encoded).</summary>
		public List<KeyValuePair<string, string>> Details { get; private set; } = new List<KeyValuePair<string, string>>();

		private bool IsDoctor { get { return Category != "P" && Category != "O"; } }
		private bool IsPatient { get { return Category == "P"; } }

		public async Task OnGetAsync()
		{
			if (SelectedId.HasValue)
			{
				/*SELECT COMMAND: reload the full list and show the details of the selected record*/
				await LoadGridAsync("", CategoryName());
				ShowDetails(SelectedId.Value);
			}
			else
			{
				/*INITIAL LOAD / SEARCH BUTTON / RADIO BUTTON CHANGE*/
				await LoadGridAsync(SearchText ?? "", CategoryName());
			}
		}

		/*EVENT HANDLER FOR DELETE BUTTON IN GRID*/
		public async Task<IActionResult> OnPostDeleteAsync(int id)
		{
			myDAL objDAL = new myDAL();

			if (IsDoctor)
			{
				if (await objDAL.DeleteDoctorAsync(id, HttpContext.RequestAborted) == 1)
				{
					Msg = " Doctor No: " + id + " Deleted";
					await LoadGridAsync("", "DOCTOR");
				}
				else
				{
					Msg = "there was some error";
				}
			}
			else if (IsPatient)
			{
				Msg = "You are not Authorized to Delete a Patient";
			}
			else
			{
				if (await objDAL.DeleteStaffAsync(id, HttpContext.RequestAborted) == 1)
				{
					Msg = "Staff No: " + id + " Deleted ";
					await LoadGridAsync("", "OTHERSTAFF");
				}
				else
				{
					Msg = "There was some Error";
				}
			}

			if (Manage == null)
			{
				await LoadGridAsync("", CategoryName());
			}

			return Page();
		}

		private string CategoryName()
		{
			if (IsDoctor) return "DOCTOR";
			if (IsPatient) return "PATIENT";
			return "OTHERSTAFF";
		}

		/*THIS FUNTION WILL LOAD THE INFORMATION OF ALL DOCTORS / PATIENTS / STAFF*/
		protected async Task LoadGridAsync(string SearchQuery, string Category)
		{
			myDAL objmyDaL = new myDAL();
			DataTable table;

			if (Category == "DOCTOR")
			{
				table = await objmyDaL.LoadDoctorAsync(SearchQuery, HttpContext.RequestAborted);

				if (table != null && table.Rows.Count > 0)
				{
					Manage = table;
				}
				else
				{
					Msg = "No Doctors to show";
				}
			}
			else if (Category == "PATIENT")
			{
				table = await objmyDaL.LoadPatientAsync(SearchQuery, HttpContext.RequestAborted);

				if (table != null && table.Rows.Count > 0)
				{
					Manage = table;
				}
				else
				{
					Msg = "No Pateints to show";
				}
			}
			else
			{
				table = await objmyDaL.LoadOtherStaffAsync(SearchQuery, HttpContext.RequestAborted);

				if (table != null && table.Rows.Count > 0)
				{
					Manage = table;
					Caption = "Other Staff Table:";
				}
				else
				{
					Msg = "No Staff Member to show";
				}
			}
		}

		/*LOADS THE PROFILE OF THE SELECTED ROW*/
		private void ShowDetails(int id)
		{
			myDAL objDAL = new myDAL();

			string name = "";
			string phone = "";
			string gender = "";
			string address = "";
			string bDate = "";

			float charges_Per_Visit = 0;
			float ReputeIndex = 0;
			int PatientsTreated = 0;
			string qualification = "";
			string specialization = "";
			int workE = 0;
			int age = 0;

			if (IsDoctor)
			{
				if (objDAL.GET_DOCTOR_PROFILE(id, ref name, ref phone, ref gender, ref charges_Per_Visit, ref ReputeIndex, ref PatientsTreated, ref qualification, ref specialization, ref workE, ref age) == 1)
				{
					Add("Name:", name);
					Add("phone:", phone);
					Add("gender:", gender);
					Add("Qualification:", qualification);
					Add(" Age:", age.ToString());
					Add("Charges:", charges_Per_Visit.ToString());
					Add("Repute index:", ReputeIndex.ToString());
				}
				else
				{
					Msg = "there was some error";
				}
			}
			else if (IsPatient)
			{
				if (objDAL.GETPATIENT(id, ref name, ref phone, ref address, ref bDate, ref age, ref gender) == 0)
				{
					Add("Name:", name);
					Add("phone:", phone);
					Add("gender:", gender);
					Add("Address:", address);
					Add(" Age:", age.ToString());
				}
				else
				{
					Msg = "there was some error";
				}
			}
			else
			{
				string designation = "";
				int s = 0;

				if (objDAL.GETSATFF(id, ref name, ref phone, ref address, ref gender, ref designation, ref s) == 1)
				{
					Add("Name:", name);
					Add("phone:", phone);
					Add("gender:", gender);
					Add("Address:", address);
					Add(" salary:", s.ToString());
				}
				else
				{
					Msg = "there was some error";
				}
			}
		}

		private void Add(string label, string value)
		{
			Details.Add(new KeyValuePair<string, string>(label, value));
		}
	}
}
