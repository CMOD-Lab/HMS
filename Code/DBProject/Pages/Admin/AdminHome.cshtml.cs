using System.Data;
using System.Threading.Tasks;
using DBProject.DAL;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBProject.Pages.Admin
{
	public class AdminHomeModel : PageModel
	{
		public string TotalPatients { get; private set; }
		public string TotalDoctors { get; private set; }
		public string TotalIncome { get; private set; }
		public DataTable DepartmentView { get; private set; }
		public DataTable AppointmentView { get; private set; }

		public async Task OnGetAsync()
		{
			await GetAdminHomeInformationAsync();
		}

		public async Task GetAdminHomeInformationAsync()
		{
			myDAL objmyDAL = new myDAL();

			DataTable[] arrTable = await objmyDAL.GetAdminHomeInformationAsync(HttpContext.RequestAborted);

			// Same mapping as the original Web Forms page (labels were bound as below).
			TotalDoctors = arrTable[0].Rows[0][0].ToString();
			TotalPatients = arrTable[1].Rows[0][0].ToString();
			TotalIncome = arrTable[2].Rows[0][0].ToString();

			DepartmentView = arrTable[3];
			AppointmentView = arrTable[4];
		}
	}
}
