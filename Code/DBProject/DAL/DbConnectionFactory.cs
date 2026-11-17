using System;
using System.Data.SqlClient;

namespace DBProject.DAL
{
	/// <summary>
	/// Central factory for database connections. Connections are targeted at an Amazon RDS Proxy
	/// endpoint, which performs server-side connection pooling/multiplexing. Settings are resolved
	/// from environment variables (12-factor); credentials are expected to be injected from
	/// AWS Secrets Manager into the environment. Falls back to the "sqlCon1" connection string
	/// in configuration for local development.
	///
	/// Environment variables:
	///   DB_CONNECTION_STRING  - full connection string (overrides everything else)
	///   RDS_PROXY_ENDPOINT    - RDS Proxy endpoint host name (or DB_HOST)
	///   DB_PORT               - default 1433
	///   DB_NAME               - default DBProject
	///   DB_USER / DB_PASSWORD - credentials (from Secrets Manager)
	///   DB_MAX_POOL_SIZE      - client-side pool upper bound, default 50
	/// </summary>
	public static class DbConnectionFactory
	{
		private static readonly Lazy<string> connectionString = new Lazy<string>(BuildConnectionString);
		private static string fallbackConnectionString;

		/// <summary>
		/// Supplies the configuration-based ("sqlCon1") connection string used when no DB_* environment
		/// variables are present (local development). Must be called at application start-up.
		/// </summary>
		public static void Configure(string configuredConnectionString)
		{
			fallbackConnectionString = configuredConnectionString;
		}

		public static SqlConnection Create()
		{
			return new SqlConnection(connectionString.Value);
		}

		private static string BuildConnectionString()
		{
			string full = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
			SqlConnectionStringBuilder builder;

			if (!string.IsNullOrEmpty(full))
			{
				builder = new SqlConnectionStringBuilder(full);
			}
			else
			{
				string host = Environment.GetEnvironmentVariable("RDS_PROXY_ENDPOINT")
					?? Environment.GetEnvironmentVariable("DB_HOST");

				if (!string.IsNullOrEmpty(host))
				{
					string port = Environment.GetEnvironmentVariable("DB_PORT") ?? "1433";
					builder = new SqlConnectionStringBuilder
					{
						DataSource = host + "," + port,
						InitialCatalog = Environment.GetEnvironmentVariable("DB_NAME") ?? "DBProject",
						UserID = Environment.GetEnvironmentVariable("DB_USER") ?? string.Empty,
						Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty,
						IntegratedSecurity = false,
						Encrypt = true
					};
				}
				else
				{
					// Local development fallback
					builder = new SqlConnectionStringBuilder(
						fallbackConnectionString
						?? Environment.GetEnvironmentVariable("ConnectionStrings__sqlCon1")
						?? throw new InvalidOperationException(
							"No database configuration found. Set DB_CONNECTION_STRING or RDS_PROXY_ENDPOINT/DB_* environment variables."));
				}
			}

			int maxPool;
			if (!int.TryParse(Environment.GetEnvironmentVariable("DB_MAX_POOL_SIZE"), out maxPool))
			{
				maxPool = 50;
			}

			builder.Pooling = true;
			builder.MaxPoolSize = maxPool;
			builder.ConnectTimeout = 30;
			builder.ApplicationName = "ClinicManagementSystem";

			return builder.ConnectionString;
		}
	}
}
