using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

// Migration note (cr-dotnet-0026): In ASP.NET Core migration, this DAL class would be
// refactored to use dependency-injected DbContext and async repository patterns
// instead of the Web Forms code-behind pattern.

namespace DBProject.DAL
{
	//Database Layer of 3 tier architecture
	// Refactored to use EF Core DbContext with RDS Proxy (cr-dotnet-0013 remediation)
	// All SqlConnection/SqlCommand/SqlDataAdapter usage replaced by EF Core managed connection
	public class myDAL
    {
        // Connection string externalized to environment variables / Parameter Store (cr-dotnet-0010)
        // EF Core DbContext manages the connection to RDS Proxy via ClinicDbContext
        // No ConfigurationManager fallback - connection built from env vars + Secrets Manager (cloud-config skill)
        private static readonly string connString =
            Environment.GetEnvironmentVariable("Database__ConnectionString")
            ?? "";

        // EF Core DbContext instance - manages connection pooling via RDS Proxy
        private ClinicDbContext _context;

        public myDAL()
        {
            var options = new DbContextOptionsBuilder<ClinicDbContext>()
                .UseSqlServer(connString)
                .Options;
            _context = new ClinicDbContext(options);
        }

        // Helper: Execute a stored procedure with parameters, return output parameter values
        private DbCommand CreateCommand(string sql, CommandType type, params DbParameter[] parameters)
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = type;
            cmd.CommandTimeout = 30;

            foreach (var p in parameters)
                cmd.Parameters.Add(p);

            return cmd;
        }

        // Helper: Fill a DataTable from a DbCommand (replaces SqlDataAdapter.Fill)
        private void FillDataTable(DbCommand cmd, DataTable table)
        {
            using (var reader = cmd.ExecuteReader())
            {
                var schema = reader.GetSchemaTable();
                if (schema != null)
                {
                    foreach (DataRow row in schema.Rows)
                    {
                        var col = new DataColumn
                        {
                            ColumnName = (string)row["ColumnName"],
                            DataType = (Type)row["DataType"]
                        };
                        table.Columns.Add(col);
                    }
                }
                while (reader.Read())
                {
                    var dr = table.NewRow();
                    for (int i = 0; i < table.Columns.Count; i++)
                        dr[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                    table.Rows.Add(dr);
                }
            }
        }

        // Helper: Execute stored procedure with output parameters using EF Core connection
        private void ExecuteStoredProcedure(string procedureName, params DbParameter[] parameters)
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = procedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 30;

                foreach (var p in parameters)
                    cmd.Parameters.Add(p);

                cmd.ExecuteNonQuery();
            }
        }

        // Helper: Fill DataTable from stored procedure using EF Core connection
        private void FillFromStoredProcedure(string procedureName, DataTable table, params DbParameter[] parameters)
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = procedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 30;

                foreach (var p in parameters)
                    cmd.Parameters.Add(p);

                using (var reader = cmd.ExecuteReader())
                {
                    var schema = reader.GetSchemaTable();
                    if (schema != null)
                    {
                        foreach (DataRow row in schema.Rows)
                        {
                            var col = new DataColumn
                            {
                                ColumnName = (string)row["ColumnName"],
                                DataType = (Type)row["DataType"]
                            };
                            table.Columns.Add(col);
                        }
                    }
                    while (reader.Read())
                    {
                        var dr = table.NewRow();
                        for (int i = 0; i < table.Columns.Count; i++)
                            dr[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                        table.Rows.Add(dr);
                    }
                }
            }
        }

        // Helper: Fill DataSet from stored procedure using EF Core connection
        private void FillDataSet(string procedureName, DataSet ds, params DbParameter[] parameters)
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = procedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 30;

                foreach (var p in parameters)
                    cmd.Parameters.Add(p);

                using (var reader = cmd.ExecuteReader())
                {
                    int tableIndex = 0;
                    do
                    {
                        var table = new DataTable();
                        var schema = reader.GetSchemaTable();
                        if (schema != null)
                        {
                            foreach (DataRow row in schema.Rows)
                            {
                                var col = new DataColumn
                                {
                                    ColumnName = (string)row["ColumnName"],
                                    DataType = (Type)row["DataType"]
                                };
                                table.Columns.Add(col);
                            }
                        }
                        while (reader.Read())
                        {
                            var dr = table.NewRow();
                            for (int i = 0; i < table.Columns.Count; i++)
                                dr[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                            table.Rows.Add(dr);
                        }
                        ds.Tables.Add(table);
                        tableIndex++;
                    } while (reader.NextResult());
                }
            }
        }

        // Helper: Execute raw SQL text using EF Core connection
        private void FillFromSql(string sql, DataTable table, params DbParameter[] parameters)
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 30;

                foreach (var p in parameters)
                    cmd.Parameters.Add(p);

                using (var reader = cmd.ExecuteReader())
                {
                    var schema = reader.GetSchemaTable();
                    if (schema != null)
                    {
                        foreach (DataRow row in schema.Rows)
                        {
                            var col = new DataColumn
                            {
                                ColumnName = (string)row["ColumnName"],
                                DataType = (Type)row["DataType"]
                            };
                            table.Columns.Add(col);
                        }
                    }
                    while (reader.Read())
                    {
                        var dr = table.NewRow();
                        for (int i = 0; i < table.Columns.Count; i++)
                            dr[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                        table.Rows.Add(dr);
                    }
                }
            }
        }

        // Helper: Create DbParameter for stored procedure parameters (cr-dotnet-0013 remediation)
        private static DbParameter CreateParam(string name, object value, SqlDbType type, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            var p = new System.Data.SqlClient.SqlParameter(name, type);
            if (size > 0)
                p.Size = size;
            p.Value = value ?? DBNull.Value;
            p.Direction = direction;
            return p;
        }

        // Helper: Execute stored procedure that returns an integer result (ExecuteNonQuery)
        private int ExecuteStoredProcedureWithResult(string procedureName, params DbParameter[] parameters)
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = procedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 30;

                foreach (var p in parameters)
                    cmd.Parameters.Add(p);

                return cmd.ExecuteNonQuery();
            }
        }


		//-----------------------------------------------------------------------------------//
		//																					 //
		//									SIGNUP											 //
		//																					 //
		//-----------------------------------------------------------------------------------//



		/*CHECKS WHETHER IT IS A VALID USER AND RETURN ITS TYPE*/
		public int validateLogin (string Email, string Password, ref int type , ref int id)
        {
            try
            {
                var statusParam = CreateParam("@status", null, SqlDbType.Int, 0, ParameterDirection.Output);
                var idParam = CreateParam("@ID", null, SqlDbType.Int, 0, ParameterDirection.Output);
                var typeParam = CreateParam("@type", null, SqlDbType.Int, 0, ParameterDirection.Output);

                ExecuteStoredProcedure("Login",
                    CreateParam("@email", Email, SqlDbType.VarChar, 30),
                    CreateParam("@password", Password, SqlDbType.VarChar, 20),
                    statusParam,
                    idParam,
                    typeParam);

                int status = (int)statusParam.Value;
                type = (int)typeParam.Value;
                id = (int)idParam.Value;

                return status;
            }
            catch
            {
                return -1;
            }
        }




		/*THIS FUNCTION WILL VALIDATE ALL THE INFORMAIION OF OF USER (PATIENT)*/
        public int validateUser (string Name, string BirthDate, string Email , string Password , string PhoneNo , string gender , string Address, ref int id)
        {
            try
            {
                var statusParam = CreateParam("@status", null, SqlDbType.Int, 0, ParameterDirection.Output);
                var idParam = CreateParam("@ID", null, SqlDbType.Int, 0, ParameterDirection.Output);

                ExecuteStoredProcedure("PatientSignup",
                    CreateParam("@name", Name, SqlDbType.VarChar, 20),
                    CreateParam("@address", Address, SqlDbType.VarChar, 40),
                    CreateParam("@gender", gender, SqlDbType.VarChar, 1),
                    CreateParam("@date", BirthDate, SqlDbType.Date),
                    CreateParam("@email", Email, SqlDbType.VarChar, 30),
                    CreateParam("@password", Password, SqlDbType.VarChar, 20),
                    CreateParam("@phone", PhoneNo, SqlDbType.Char, 15),
                    statusParam,
                    idParam);

                int status = (int)statusParam.Value;

                if (status != 0)
                {
                    id = (int)idParam.Value;
                }

                return status;
            }
            catch
            {
                return -1;
            }
        }







        //-----------------------------------------------------------------------------------//
        //                                                                                   //
        //                                       ADMIN                                       //
        //                                                                                   //
        //-----------------------------------------------------------------------------------//



        /*THIS FUNCTION CHECKS WHEATHER EMAIL OF A DOCTOR ALREADY EXISTS IN THE DATABASE */

        public int DoctorEmailAlreadyExist(string Email)
        {
            int status = 0;
            var statusParam = CreateParam("@status", null, SqlDbType.Int, 0, ParameterDirection.Output);

            ExecuteStoredProcedure("CheckDoctorEmail",
                CreateParam("@Email", Email, SqlDbType.VarChar, 30),
                statusParam);

            status = (int)statusParam.Value;
            return status;
        }





        /*THIS FUNCTION WILL ADD THE DOCTOR TO THE DATA BASE */
        public void AddDoctor(string Name, string Email, string Password, string BirthDate, int dept, string Phone, char gender, string Address, int exp, int salary, int Charges_per_visit, string spec, string qual)
        {
            ExecuteStoredProcedure("AddDoctor",
                CreateParam("@Name", Name, SqlDbType.VarChar, 30),
                CreateParam("@Email", Email, SqlDbType.VarChar, 30),
                CreateParam("@Password", Password, SqlDbType.VarChar, 30),
                CreateParam("@BirthDate", BirthDate, SqlDbType.Date),
                CreateParam("@dept", dept, SqlDbType.VarChar, 30),
                CreateParam("@gender", gender, SqlDbType.VarChar, 1),
                CreateParam("@Address", Address, SqlDbType.VarChar, 30),
                CreateParam("@Exp", exp, SqlDbType.VarChar, 30),
                CreateParam("@Salary", salary, SqlDbType.VarChar, 30),
                CreateParam("@charges", Charges_per_visit, SqlDbType.VarChar, 30),
                CreateParam("@phone", Phone, SqlDbType.VarChar, 30),
                CreateParam("@spec", spec, SqlDbType.VarChar, 30),
                CreateParam("@qual", qual, SqlDbType.VarChar, 30));
        }





        /*THIS FUNCTION WILL ADD STAFF TO THE DATA BASE*/
        public int AddStaff(string Name, string BirthDate, string Phone, char gender, string Address, int salary, string Qual, string Designation)
        {
            try
            {
                ExecuteStoredProcedure("AddStaff",
                    CreateParam("@Name", Name, SqlDbType.VarChar, 30),
                    CreateParam("@BirthDate", BirthDate, SqlDbType.Date),
                    CreateParam("@Phone", Phone, SqlDbType.VarChar, 30),
                    CreateParam("@gender", gender, SqlDbType.VarChar, 1),
                    CreateParam("@salary", salary, SqlDbType.Int, 30),
                    CreateParam("@Designation", Designation, SqlDbType.VarChar, 30),
                    CreateParam("@Qualification", Qual, SqlDbType.VarChar, 1),
                    CreateParam("@Address", Address, SqlDbType.VarChar, 50));

                return 1;
            }
            catch
            {
                return -1;
            }
        }



		/*THIS FUNCTION WILL RUN MULTIPLE QUERIES AND GET ALL THE INFORMATION NEEDED TO DISPLAY AT ADMIN HOME*/
        public void GetAdminHomeInformation(ref DataTable[] arrTable)
        {
            FillFromSql("SELECT * FROM Total_Patient", arrTable[0]);
            FillFromSql("SELECT * FROM Total_Doctors", arrTable[1]);
            FillFromSql("SELECT * FROM Income", arrTable[2]);
            FillFromSql("SELECT * FROM Department_View", arrTable[3]);
            FillFromSql("SELECT * FROM Appointment_view", arrTable[4]);
        }






        /*THIS FUNCTION IS INTENDED TO DELETE DOCTOR BUT SECRETLY IT ONLY UPDATE THE STATUS*/
        public int DeleteDoctor(int id)
        {
            try
            {
                ExecuteStoredProcedure("DeleteDoctor",
                    CreateParam("@id", id, SqlDbType.Int));
            }
            catch
            {
                return -1;
            }

            return 1;
        }



        /*THIS FUNCTION WILL DELLETE STAFF FROM THE DOCTOR */
        public int DeleteStaff(int id)
        {
            try
            {
                ExecuteStoredProcedure("DELETESTAFF",
                    CreateParam("@id", id, SqlDbType.Int));
            }
            catch
            {
                return -1;
            }

            return 1;
        }


        /*LOADS THE TABLE OF DOCTOR / SPECIFIED DOCTORS ON THE BASIS OF SEARCH QUERY*/
        public void LoadDoctor(ref DataTable table, String SearchQuery)
        {
            if (SearchQuery == "")
            {
                FillFromSql(
                "SELECT Doctor.DoctorID as ID , Doctor.Name , D.DeptName as Department FROM Doctor JOIN Department D ON D.DeptNo = Doctor.DeptNo" +
                " WHERE Doctor.Status = 1",
                table);
            }
            else
            {
                FillFromSql(
                "SELECT a.DoctorID as ID,  a.Name, D.DeptName as Department FROM department D join (SELECT * FROM Doctor WHERE Doctor.Status = 1 AND Doctor.Name like  '%' + @DName + '%')  a ON a.DeptNo = D.DeptNo",
                table,
                CreateParam("@DName", SearchQuery, SqlDbType.VarChar, 0));
            }
        }






        /*LOADS THE TABLE OF PATIENT ON THE BASIS OF SEARCH QUERY*/
        /*FOR EMPTY QUERY RETURN ALL INFORMATION OTHERWISE RETURN ONLY REQUIRED TUPLE*/
        public void LoadPatient(ref DataTable table, String SearchQuery)
        {
            if (SearchQuery == "")
            {
                FillFromSql("SELECT * FROM PATIENT_VIEW", table);
            }
            else
            {
                FillFromSql("SELECT Patient.PatientID, Patient.Name, Patient.Phone from Patient" +
                " WHERE patient.name like '%' + @SName + '%' ",
                table,
                CreateParam("@SName", SearchQuery.Trim(), SqlDbType.VarChar, 0));
            }
        }





        /*LOADS THE TABLE OF OTHER STAFF ON THE BASIS OF SEARCH QUERY*/
        /*IF THE QUERY IS EMPTY THEN LOAD ALL STAFF MEMBERS OTHER WISE ONLY SPECIFIED*/
        public void LoadOtherStaff(ref DataTable table, String SearchQuery)
        {
            if (SearchQuery == "")
            {
                FillFromSql("SELECT * FROM STAFF_VIEW", table);
            }
            else
            {
                FillFromSql("SELECT StaffID as ID , Name , Designation from OtherStaff WHERE Name like '%' + @pName + '%'",
                table,
                CreateParam("@pName", SearchQuery.Trim(), SqlDbType.VarChar, 0));
            }
        }





        public int GETPATIENT(int pid, ref string name, ref string phone, ref string address, ref string birthDate, ref int age, ref string gender)
        {
            try
            {
                var nameParam = CreateParam("@name", null, SqlDbType.VarChar, 20, ParameterDirection.Output);
                var phoneParam = CreateParam("@phone", null, SqlDbType.Char, 15, ParameterDirection.Output);
                var birthDateParam = CreateParam("@birthDate", null, SqlDbType.VarChar, 10, ParameterDirection.Output);
                var addressParam = CreateParam("@address", null, SqlDbType.VarChar, 40, ParameterDirection.Output);
                var ageParam = CreateParam("@age", null, SqlDbType.Int, 0, ParameterDirection.Output);
                var genderParam = CreateParam("@gender", null, SqlDbType.Char, 1, ParameterDirection.Output);

                ExecuteStoredProcedure("RetrievePatientData",
                    CreateParam("@ID", pid, SqlDbType.Int),
                    nameParam,
                    phoneParam,
                    birthDateParam,
                    addressParam,
                    ageParam,
                    genderParam);

                name = (string)nameParam.Value.ToString();
                phone = (string)phoneParam.Value.ToString();
                address = (string)addressParam.Value.ToString();
                birthDate = (string)birthDateParam.Value.ToString();
                age = Convert.ToInt32(ageParam.Value);
                gender = (string)genderParam.Value.ToString();

                return 0;
            }
            catch
            {
                return -1;
            }
        }






        public int GET_DOCTOR_PROFILE(int dID, ref string name, ref string phone, ref string gender, ref float charges_Per_Visit, ref float ReputeIndex, ref int PatientsTreated, ref string qualification, ref string specialization, ref int workE, ref int age)
        {
            try
            {
                var nameParam = CreateParam("@name", null, SqlDbType.VarChar, 20, ParameterDirection.Output);
                var phoneParam = CreateParam("@phone", null, SqlDbType.VarChar, 15, ParameterDirection.Output);
                var genderParam = CreateParam("@gender", null, SqlDbType.VarChar, 2, ParameterDirection.Output);
                var chargesParam = CreateParam("@charges", null, SqlDbType.Float, 0, ParameterDirection.Output);
                var riParam = CreateParam("@RI", null, SqlDbType.Float, 0, ParameterDirection.Output);
                var pTreatedParam = CreateParam("@PTreated", null, SqlDbType.Int, 0, ParameterDirection.Output);
                var qualParam = CreateParam("@qualification", null, SqlDbType.VarChar, 100, ParameterDirection.Output);
                var specParam = CreateParam("@specialization", null, SqlDbType.VarChar, 50, ParameterDirection.Output);
                var workEParam = CreateParam("@workE", null, SqlDbType.Int, 0, ParameterDirection.Output);
                var ageParam = CreateParam("@age", null, SqlDbType.Int, 0, ParameterDirection.Output);

                ExecuteStoredProcedure("GET_DOCTOR_PROFILE",
                    CreateParam("@dID", dID, SqlDbType.Int),
                    nameParam,
                    phoneParam,
                    genderParam,
                    chargesParam,
                    riParam,
                    pTreatedParam,
                    qualParam,
                    specParam,
                    workEParam,
                    ageParam);

                name = (string)nameParam.Value;
                phone = (string)phoneParam.Value;
                gender = (string)genderParam.Value;
                charges_Per_Visit = Convert.ToSingle(chargesParam.Value);
                ReputeIndex = Convert.ToSingle(riParam.Value);
                PatientsTreated = (int)pTreatedParam.Value;
                qualification = (string)qualParam.Value;
                specialization = (string)specParam.Value;
                workE = (int)workEParam.Value;
                age = (int)ageParam.Value;
            }
            catch
            {
                return -1;
            }

            return 1;
        }



        public int GETSATFF(int id, ref string name, ref string phone, ref string address, ref string gender, ref string desig, ref int sal)
        {
            var nameParam = CreateParam("@name", null, SqlDbType.VarChar, 20, ParameterDirection.Output);
            var phoneParam = CreateParam("@phone", null, SqlDbType.VarChar, 15, ParameterDirection.Output);
            var genderParam = CreateParam("@gender", null, SqlDbType.VarChar, 2, ParameterDirection.Output);
            var addressParam = CreateParam("@address", null, SqlDbType.VarChar, 100, ParameterDirection.Output);
            var desigParam = CreateParam("@desig", null, SqlDbType.VarChar, 100, ParameterDirection.Output);
            var salParam = CreateParam("@sal", null, SqlDbType.Int, 0, ParameterDirection.Output);

            ExecuteStoredProcedure("GET_STAFF",
                CreateParam("@id", id, SqlDbType.Int),
                nameParam,
                phoneParam,
                genderParam,
                addressParam,
                desigParam,
                salParam);

            name = (string)nameParam.Value;
            phone = (string)phoneParam.Value;
            gender = (string)genderParam.Value;
            address = (string)addressParam.Value;
            desig = (string)desigParam.Value;
            sal = (int)salParam.Value;

            return 1;
        }






		//-----------------------------------------------------------------------------------//
		//                                                                                   //
		//                                       PATIENT                                     //
		//                                                                                   //
		//-----------------------------------------------------------------------------------//









		/*-------------------DISPLAYS PATIENT INFORMATION AT PATIENT HOME--------------------------------------- */

		public int patientInfoDisplayer(int pid, ref string name, ref string phone, ref string address, ref string birthDate, ref int age, ref string gender)
		{
			try
			{
				var nameParam = CreateParam("@name", null, SqlDbType.VarChar, 20, ParameterDirection.Output);
				var phoneParam = CreateParam("@phone", null, SqlDbType.Char, 15, ParameterDirection.Output);
				var birthDateParam = CreateParam("@birthDate", null, SqlDbType.VarChar, 10, ParameterDirection.Output);
				var addressParam = CreateParam("@address", null, SqlDbType.VarChar, 40, ParameterDirection.Output);
				var ageParam = CreateParam("@age", null, SqlDbType.Int, 0, ParameterDirection.Output);
				var genderParam = CreateParam("@gender", null, SqlDbType.Char, 1, ParameterDirection.Output);

				ExecuteStoredProcedure("RetrievePatientData",
					CreateParam("@ID", pid, SqlDbType.Int),
					nameParam,
					phoneParam,
					birthDateParam,
					addressParam,
					ageParam,
					genderParam);

				name = (string)nameParam.Value;
				phone = (string)phoneParam.Value;
				address = (string)addressParam.Value;
				birthDate = (string)birthDateParam.Value;
				age = (int)ageParam.Value;
				gender = (string)genderParam.Value;

				return 0;
			}
			catch
			{
				return -1;
			}
		}


		/*---------------------------GENERATE BILL HISTORY--------------------------------------*/

		public int getBillHistory(int id, ref DataTable result)
		{
			DataSet ds = new DataSet();
			try
			{
				var countParam = CreateParam("@count", null, SqlDbType.Int, 0, ParameterDirection.Output);

				ExecuteStoredProcedure("RetrieveBillHistory",
					CreateParam("@pId", id, SqlDbType.Int),
					countParam);

				// Now fill the result set using FromSqlRaw equivalent
				// Use EF Core to execute the stored procedure and read results
				var table = new DataTable();
				FillFromStoredProcedure("RetrieveBillHistory", table,
					CreateParam("@pId", id, SqlDbType.Int));
				result = table;
				return (int)countParam.Value;
			}
			catch
			{
				return -1;
			}
		}





		//-------------------------------------CURRENT APPOINTMENTS------------------------------------------//

		public int appointmentTodayDisplayer(int pid, ref string dName, ref string timings)
		{
			try
			{
				var countParam = CreateParam("@count", null, SqlDbType.Int, 0, ParameterDirection.Output);
				var timingsParam = CreateParam("@timings", null, SqlDbType.VarChar, 30, ParameterDirection.Output);
				var dNameParam = CreateParam("@dName", null, SqlDbType.VarChar, 30, ParameterDirection.Output);

				ExecuteStoredProcedure("RetrieveCurrentAppointment",
					CreateParam("@pid", pid, SqlDbType.Int),
					countParam,
					timingsParam,
					dNameParam);

				int status = (int)countParam.Value;

				if (status == 0)
				{
					return status;
				}
				else
				{
					dName = (string)dNameParam.Value;
					timings = (string)timingsParam.Value;
					return status;
				}
			}
			catch
			{
				return -1;
			}
		}




		//-------------------------------------TREATMENT HISTORY------------------------------------------//
		public int getTreatmentHistory(int id, ref DataTable result)
		{
			DataSet ds = new DataSet();
			try
			{
				var countParam = CreateParam("@count", null, SqlDbType.Int, 0, ParameterDirection.Output);

				ExecuteStoredProcedure("RetrieveTreatmentHistory",
					CreateParam("@pId", id, SqlDbType.Int),
					countParam);

				var table = new DataTable();
				FillFromStoredProcedure("RetrieveTreatmentHistory", table,
					CreateParam("@pId", id, SqlDbType.Int));
				result = table;
				return (int)countParam.Value;
			}
			catch
			{
				return -1;
			}
		}




		/*-------------------------TAKE APPOINMENT------------------------------------*/
		public int getdeptInfo(ref DataTable result)
		{
			DataSet ds = new DataSet();
			try
			{
				FillFromSql("select* from deptInfo", result);
				return 1;
			}
			catch
			{
				return -1;
			}
		}




		//-------------------------------------VIEW DOCTORS------------------------------------------//

		public int getDeptDoctorInfo(string deptName, ref DataTable result)
		{
			DataSet ds = new DataSet();
			try
			{
				FillFromStoredProcedure("RetrieveDeptDoctorInfo", result,
					CreateParam("@deptName", deptName, SqlDbType.VarChar, 30));
				return 1;
			}
			catch
			{
				return -1;
			}
		}






		//-------------------------------------DOCTOR PROFILE------------------------------------------//


		public int doctorInfoDisplayer(int dID, ref string name, ref string phone, ref string gender, ref float charges_Per_Visit, ref float ReputeIndex, ref int PatientsTreated, ref string qualification, ref string specialization, ref int workE, ref int age)
		{
			try
			{
				var nameParam = CreateParam("@name", null, SqlDbType.VarChar, 20, ParameterDirection.Output);
				var phoneParam = CreateParam("@phone", null, SqlDbType.VarChar, 15, ParameterDirection.Output);
				var genderParam = CreateParam("@gender", null, SqlDbType.VarChar, 2, ParameterDirection.Output);
				var chargesParam = CreateParam("@charges", null, SqlDbType.Float, 0, ParameterDirection.Output);
				var riParam = CreateParam("@RI", null, SqlDbType.Float, 0, ParameterDirection.Output);
				var pTreatedParam = CreateParam("@PTreated", null, SqlDbType.Int, 0, ParameterDirection.Output);
				var qualParam = CreateParam("@qualification", null, SqlDbType.VarChar, 100, ParameterDirection.Output);
				var specParam = CreateParam("@specialization", null, SqlDbType.VarChar, 50, ParameterDirection.Output);
				var workEParam = CreateParam("@workE", null, SqlDbType.Int, 0, ParameterDirection.Output);
				var ageParam = CreateParam("@age", null, SqlDbType.Int, 0, ParameterDirection.Output);

				ExecuteStoredProcedure("RetrieveDoctorData",
					CreateParam("@dID", dID, SqlDbType.Int),
					nameParam,
					phoneParam,
					genderParam,
					chargesParam,
					riParam,
					pTreatedParam,
					qualParam,
					specParam,
					workEParam,
					ageParam);

				name = (string)nameParam.Value;
				phone = (string)phoneParam.Value;
				gender = (string)genderParam.Value;
				charges_Per_Visit = Convert.ToSingle(chargesParam.Value);
				ReputeIndex = Convert.ToSingle(riParam.Value);
				PatientsTreated = (int)pTreatedParam.Value;
				qualification = (string)qualParam.Value;
				specialization = (string)specParam.Value;
				workE = (int)workEParam.Value;
				age = (int)ageParam.Value;

				return 0;
			}
			catch
			{
				return -1;
			}
		}


		//-------------------------------------APPOINTMENT TAKER------------------------------------------//

		public int getFreeSlots(int dID, int pID, ref DataTable result)
		{
			DataSet ds = new DataSet();
			try
			{
				var countParam = CreateParam("@count", null, SqlDbType.Int, 0, ParameterDirection.Output);

				ExecuteStoredProcedure("RetrieveFreeSlots",
					CreateParam("@dID", dID, SqlDbType.Int),
					CreateParam("@pID", pID, SqlDbType.Int),
					countParam);

				var table = new DataTable();
				FillFromStoredProcedure("RetrieveFreeSlots", table,
					CreateParam("@dID", dID, SqlDbType.Int),
					CreateParam("@pID", pID, SqlDbType.Int));
				result = table;
				return (int)countParam.Value;
			}
			catch
			{
				return -1;
			}
		}




		//-------------------------------------APPOINTMENT REQUEST SENT------------------------------------------//

		public int insertAppointment(int dID, int pID, int freeSlot, ref string mes)
		{
			try
			{
				ExecuteStoredProcedure("insertInAppointmentTable",
					CreateParam("@dID", dID, SqlDbType.Int),
					CreateParam("@pID", pID, SqlDbType.Int),
					CreateParam("@freeSlot", freeSlot, SqlDbType.Int));
				mes = "";
				return 0;
			}
			catch
			{
				return -1;
			}
		}





		//-------------------------------------PATIENT NOTIFICATIONS------------------------------------------//

		public int getNotifications(int pid, ref string dName, ref string timings)
		{
			try
			{
				var countParam = CreateParam("@count", null, SqlDbType.Int, 0, ParameterDirection.Output);
				var timingsParam = CreateParam("@timings", null, SqlDbType.VarChar, 30, ParameterDirection.Output);
				var dNameParam = CreateParam("@dName", null, SqlDbType.VarChar, 30, ParameterDirection.Output);

				ExecuteStoredProcedure("RetrievePatientNotifications",
					CreateParam("@pId", pid, SqlDbType.Int),
					countParam,
					timingsParam,
					dNameParam);

				int status = (int)countParam.Value;

				if (status == 0)
				{
					return status;
				}
				else
				{
					dName = (string)dNameParam.Value;
					timings = (string)timingsParam.Value;
					return status;
				}
			}
			catch
			{
				return -1;
			}
		}






		//-------------------------------------PATIENT FEEDBACK------------------------------------------//
		//-------------------------------------FUNCTION 1------------------------------------------//

		public int isFeedbackPending(int pid, ref string dName, ref string timings, ref int aID)
		{
			try
			{
				var countParam = CreateParam("@count", null, SqlDbType.Int, 0, ParameterDirection.Output);
				var timingsParam = CreateParam("@timings", null, SqlDbType.VarChar, 30, ParameterDirection.Output);
				var dNameParam = CreateParam("@dName", null, SqlDbType.VarChar, 30, ParameterDirection.Output);
				var aIDParam = CreateParam("@aID", null, SqlDbType.Int, 0, ParameterDirection.Output);

				ExecuteStoredProcedure("RetrievePendingFeedback",
					CreateParam("@pId", pid, SqlDbType.Int),
					countParam,
					timingsParam,
					dNameParam,
					aIDParam);

				int status = (int)countParam.Value;

				if (status == 0)
				{
					return status;
				}
				else
				{
					dName = (string)dNameParam.Value;
					timings = (string)timingsParam.Value;
					aID = (int)aIDParam.Value;
					return status;
				}
			}
			catch
			{
				return -1;
			}
		}




		//-------------------------------------FUNCTION 2------------------------------------------//

		public int givePendingFeedback(int aID)
		{
			try
			{
				ExecuteStoredProcedure("storeFeedback",
					CreateParam("@aId", aID, SqlDbType.Int));
				return 0;
			}
			catch
			{
				return -1;
			}
		}















		//-----------------------------------------------------------------------------------//
		//                                                                                   //
		//                                       DOCTOR                                      //
		//                                                                                   //
		//-----------------------------------------------------------------------------------//






		/*THIS FUNCITON WILL RETRIEVE THE INFORMATION OF CURRENT LOGGED IN DOCTOR*/
		public int docinfo_DAL(int doctorid, ref DataTable result)
		{
			try
			{
				FillFromStoredProcedure("Doctor_Information_By_ID1", result,
					CreateParam("@ID", doctorid, SqlDbType.Int));
				return 1;
			}
			catch (SqlException ex)
			{
				return 0;
			}
			catch
			{
				return 0;
			}
		}






		/*THIS FUNCTION WILL RETURN PENDING APPOINTMENT FORM THE DATABASE IN THE FORM OF DATASET*/
		public void GetAllpendingappointments_DAL(int doctorid, ref DataTable DT)
		{
			try
			{
				FillFromStoredProcedure("PENDING_APPOINTMENTS2", DT,
					CreateParam("@DOCTOR_ID", doctorid, SqlDbType.Int));
			}
			catch (SqlException ex)
			{
				Console.WriteLine("SQL Error" + ex.Message.ToString());
			}
		}


		/*THIS FUNCTION WILL BE CALLED WHEN DOCTOR APPROVE THE REQUEST OF PATIENT*/
		public int UpdateAppointment_DAL(int Appointmentid)
		{
			int result = 0;
			try
			{
				result = ExecuteStoredProcedureWithResult("APPROVE_APPOINTMENT",
					CreateParam("@APPOINT_ID", Appointmentid, SqlDbType.Int));
			}
			catch (SqlException ex)
			{
				Console.WriteLine("SQL Error" + ex.Message.ToString());
			}
			return result;
		}



		/*DELETES THE APPOINTMENT*/
		public int Deleteappointment_DAL(int appointmentid)
		{
			try
			{
				ExecuteStoredProcedure("delete_APPOINTMENT",
					CreateParam("@APPOINT_ID", appointmentid, SqlDbType.Int));
			}
			catch (SqlException ex)
			{
				Console.WriteLine("SQL Error" + ex.Message.ToString());
				return -1;
			}

			return 1;
		}




		/*THIS FUNTION RETURN CURRENT DAY APPONTMENT*/
		public int search_patient_DAL(int did, ref DataTable result)
		{
			try
			{
				FillFromStoredProcedure("TODAYS_APPOINTMENTS", result,
					CreateParam("@DOC_ID", did, SqlDbType.Int));
				return 1;
			}
			catch
			{
			}

			return 1;
		}






		/*UPDATE THE PRESCRIPTION WHEN APPOINTMENT IS GOING ON BY DOCTOR*/
		public int update_prescription_DAL(int did, int appointid, string disease, string progres, string prescrip)
		{
			try
			{
				ExecuteStoredProcedure("UpdatePrescription",
					CreateParam("@docId", did, SqlDbType.Int),
					CreateParam("@appointid", appointid, SqlDbType.Int),
					CreateParam("@Disease", disease, SqlDbType.VarChar, 30),
					CreateParam("@progress", progres, SqlDbType.VarChar, 50),
					CreateParam("@prescription", prescrip, SqlDbType.VarChar, 60));
			}
			catch
			{
				return 0;
			}

			return 1;
		}


		/*GENERATES BILL*/

		public int generate_bill_DAL(int docid, ref DataTable result)
		{
			try
			{
				FillFromStoredProcedure("generate_bill", result,
					CreateParam("@dId", docid, SqlDbType.Int));
				return 1;
			}
			catch
			{
				return -1;
			}
		}




		public void paid_bill_DAL(int did, int appoint)
		{
			ExecuteStoredProcedure("finishedPaid",
				CreateParam("@docId", did, SqlDbType.Int),
				CreateParam("@appointid", appoint, SqlDbType.Int));
		}


        public void Unpaid_bill_DAL(int did, int appoint)
        {
			ExecuteStoredProcedure("finishedUnPaid",
				CreateParam("@docId", did, SqlDbType.Int),
				CreateParam("@appointid", appoint, SqlDbType.Int));
        }


        public int getPHistory(int id, ref DataTable result)
        {
			try
			{
				FillFromStoredProcedure("RetrievePHistory", result,
					CreateParam("@dId", id, SqlDbType.Int));
				return 1;
			}
			catch
			{
				return -1;
			}
        }


    }


}
