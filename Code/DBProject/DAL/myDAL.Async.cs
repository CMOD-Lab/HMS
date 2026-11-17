using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace DBProject.DAL
{
	/// <summary>
	/// Task-based (async) data access used by the Razor Pages that render tabular data (the replacement
	/// for Web Forms GridView data binding). All I/O is awaited so request threads are never blocked,
	/// which prevents thread-pool exhaustion under load. Connections come from DbConnectionFactory
	/// (Amazon RDS Proxy endpoint + client-side pooling).
	/// </summary>
	public partial class myDAL
	{
		/// <summary>Executes the command asynchronously and loads the first result set into a DataTable.</summary>
		private static async Task<DataTable> ReadTableAsync(SqlCommand cmd, CancellationToken ct)
		{
			DataTable table = new DataTable();
			using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
			{
				table.Load(reader);
			}
			return table;
		}

		/*ADMIN HOME: runs the summary queries and returns the 5 result tables*/
		public async Task<DataTable[]> GetAdminHomeInformationAsync(CancellationToken ct = default(CancellationToken))
		{
			string[] queries =
			{
				"SELECT * FROM Total_Patient",
				"SELECT * FROM Total_Doctors",
				"SELECT * FROM Income",
				"SELECT * FROM Department_View",
				"SELECT * FROM Appointment_view"
			};

			DataTable[] tables = new DataTable[queries.Length];

			using (SqlConnection con = DbConnectionFactory.Create())
			{
				await con.OpenAsync(ct).ConfigureAwait(false);

				for (int i = 0; i < queries.Length; i++)
				{
					using (SqlCommand cmd = new SqlCommand(queries[i], con))
					{
						tables[i] = await ReadTableAsync(cmd, ct).ConfigureAwait(false);
					}
				}
			}

			return tables;
		}

		/*MANAGE CLINIC: DOCTOR / PATIENT / OTHER STAFF LISTS*/
		public async Task<DataTable> LoadDoctorAsync(string searchQuery, CancellationToken ct = default(CancellationToken))
		{
			using (SqlConnection con = DbConnectionFactory.Create())
			using (SqlCommand cmd = new SqlCommand())
			{
				cmd.Connection = con;

				if (string.IsNullOrEmpty(searchQuery))
				{
					cmd.CommandText = "SELECT Doctor.DoctorID as ID , Doctor.Name , D.DeptName as Department FROM Doctor JOIN Department D ON D.DeptNo = Doctor.DeptNo" +
						" WHERE Doctor.Status = 1";
				}
				else
				{
					cmd.CommandText = "SELECT a.DoctorID as ID,  a.Name, D.DeptName as Department FROM department D join (SELECT * FROM Doctor WHERE Doctor.Status = 1 AND Doctor.Name like  '%' + @DName + '%')  a ON a.DeptNo = D.DeptNo";
					cmd.Parameters.AddWithValue("@DName", searchQuery);
				}

				await con.OpenAsync(ct).ConfigureAwait(false);
				return await ReadTableAsync(cmd, ct).ConfigureAwait(false);
			}
		}

		public async Task<DataTable> LoadPatientAsync(string searchQuery, CancellationToken ct = default(CancellationToken))
		{
			using (SqlConnection con = DbConnectionFactory.Create())
			using (SqlCommand cmd = new SqlCommand())
			{
				cmd.Connection = con;

				if (string.IsNullOrEmpty(searchQuery))
				{
					cmd.CommandText = "SELECT * FROM PATIENT_VIEW";
				}
				else
				{
					cmd.CommandText = "SELECT Patient.PatientID, Patient.Name, Patient.Phone from Patient" +
						" WHERE patient.name like '%' + @SName + '%' ";
					cmd.Parameters.AddWithValue("@SName", searchQuery.Trim());
				}

				await con.OpenAsync(ct).ConfigureAwait(false);
				return await ReadTableAsync(cmd, ct).ConfigureAwait(false);
			}
		}

		public async Task<DataTable> LoadOtherStaffAsync(string searchQuery, CancellationToken ct = default(CancellationToken))
		{
			using (SqlConnection con = DbConnectionFactory.Create())
			using (SqlCommand cmd = new SqlCommand())
			{
				cmd.Connection = con;

				if (string.IsNullOrEmpty(searchQuery))
				{
					cmd.CommandText = "SELECT * FROM STAFF_VIEW";
				}
				else
				{
					cmd.CommandText = "SELECT StaffID as ID , Name , Designation from OtherStaff WHERE Name like '%' + @pName + '%'";
					cmd.Parameters.AddWithValue("@PName", searchQuery.Trim());
				}

				await con.OpenAsync(ct).ConfigureAwait(false);
				return await ReadTableAsync(cmd, ct).ConfigureAwait(false);
			}
		}

		public async Task<int> DeleteDoctorAsync(int id, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("DeleteDoctor", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
						await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
					}
				}
			}
			catch (SqlException)
			{
				return -1;
			}

			return 1;
		}

		public async Task<int> DeleteStaffAsync(int id, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("DELETESTAFF", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
						await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
					}
				}
			}
			catch (SqlException)
			{
				return -1;
			}

			return 1;
		}

		/*DOCTOR: TODAY'S APPOINTMENTS (PatientHistory page). Status is 1 on success, as in search_patient_DAL.*/
		public async Task<(int Status, DataTable Table)> SearchPatientAsync(int did, CancellationToken ct = default(CancellationToken))
		{
			DataTable table = null;
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("TODAYS_APPOINTMENTS", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@DOC_ID", SqlDbType.Int).Value = did;
						table = await ReadTableAsync(cmd, ct).ConfigureAwait(false);
					}
				}
			}
			catch (SqlException)
			{
				// matches the synchronous version: errors are swallowed and the grid is empty
			}

			return (1, table);
		}

		/*DOCTOR: PENDING APPOINTMENTS*/
		public async Task<DataTable> GetAllPendingAppointmentsAsync(int doctorId, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("PENDING_APPOINTMENTS2", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@DOCTOR_ID", SqlDbType.Int).Value = doctorId;
						return await ReadTableAsync(cmd, ct).ConfigureAwait(false);
					}
				}
			}
			catch (SqlException ex)
			{
				Console.WriteLine("SQL Error" + ex.Message.ToString());
				return null;
			}
		}

		public async Task<int> UpdateAppointmentAsync(int appointmentId, CancellationToken ct = default(CancellationToken))
		{
			int result = 0;
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("APPROVE_APPOINTMENT", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@APPOINT_ID", SqlDbType.Int).Value = appointmentId;
						result = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
					}
				}
			}
			catch (SqlException ex)
			{
				Console.WriteLine("SQL Error" + ex.Message.ToString());
			}

			return result;
		}

		public async Task<int> DeleteAppointmentAsync(int appointmentId, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("delete_APPOINTMENT", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@APPOINT_ID", SqlDbType.Int).Value = appointmentId;
						await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
					}
				}
			}
			catch (SqlException ex)
			{
				Console.WriteLine("SQL Error" + ex.Message.ToString());
				return -1;
			}

			return 1;
		}

		/*DOCTOR: PREVIOUS PATIENT HISTORY. Status is 1 on success, -1 on error.*/
		public async Task<(int Status, DataTable Table)> GetPHistoryAsync(int id, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("RetrievePHistory", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@dId", SqlDbType.Int).Value = id;
						DataTable table = await ReadTableAsync(cmd, ct).ConfigureAwait(false);
						return (1, table);
					}
				}
			}
			catch (SqlException)
			{
				return (-1, null);
			}
		}

		/*PATIENT: FREE SLOTS OF A DOCTOR. Status is the slot count, or -1 on error.*/
		public async Task<(int Status, DataTable Table)> GetFreeSlotsAsync(int dID, int pID, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("RetrieveFreeSlots", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@dID", SqlDbType.Int).Value = dID;
						cmd.Parameters.Add("@pID", SqlDbType.Int).Value = pID;
						cmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

						DataTable table = await ReadTableAsync(cmd, ct).ConfigureAwait(false);

						// output parameters are available once the reader has been closed
						return ((int)cmd.Parameters["@count"].Value, table);
					}
				}
			}
			catch (SqlException)
			{
				return (-1, null);
			}
		}

		/*PATIENT: BILL HISTORY. Status is the bill count, or -1 on error.*/
		public async Task<(int Status, DataTable Table)> GetBillHistoryAsync(int id, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("RetrieveBillHistory", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@pId", SqlDbType.Int).Value = id;
						cmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

						DataTable table = await ReadTableAsync(cmd, ct).ConfigureAwait(false);

						return ((int)cmd.Parameters["@count"].Value, table);
					}
				}
			}
			catch (SqlException)
			{
				return (-1, null);
			}
		}

		/*PATIENT: TREATMENT HISTORY. Status is the appointment count, or -1 on error.*/
		public async Task<(int Status, DataTable Table)> GetTreatmentHistoryAsync(int id, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("RetrieveTreatmentHistory", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@pId", SqlDbType.Int).Value = id;
						cmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

						DataTable table = await ReadTableAsync(cmd, ct).ConfigureAwait(false);

						return ((int)cmd.Parameters["@count"].Value, table);
					}
				}
			}
			catch (SqlException)
			{
				return (-1, null);
			}
		}

		/*PATIENT: DEPARTMENTS. Status is 1 on success, -1 on error.*/
		public async Task<(int Status, DataTable Table)> GetDeptInfoAsync(CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("select* from deptInfo", con))
					{
						cmd.CommandType = CommandType.Text;
						DataTable table = await ReadTableAsync(cmd, ct).ConfigureAwait(false);
						return (1, table);
					}
				}
			}
			catch (SqlException)
			{
				return (-1, null);
			}
		}

		/*PATIENT: DOCTORS OF A DEPARTMENT. Status is 1 on success, -1 on error.*/
		public async Task<(int Status, DataTable Table)> GetDeptDoctorInfoAsync(string deptName, CancellationToken ct = default(CancellationToken))
		{
			try
			{
				using (SqlConnection con = DbConnectionFactory.Create())
				{
					await con.OpenAsync(ct).ConfigureAwait(false);
					using (SqlCommand cmd = new SqlCommand("RetrieveDeptDoctorInfo", con))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add("@deptName", SqlDbType.VarChar, 30).Value = deptName;
						DataTable table = await ReadTableAsync(cmd, ct).ConfigureAwait(false);
						return (1, table);
					}
				}
			}
			catch (SqlException)
			{
				return (-1, null);
			}
		}
	}
}
