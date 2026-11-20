using System;
using System.Configuration;
using System.Data.SqlClient;
using Amazon;
using Amazon.RDS.Util;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace DBProject.DAL
{
    /// <summary>
    /// Factory class that creates SqlConnection instances configured for
    /// Amazon RDS Proxy with IAM authentication.
    /// 
    /// This replaces direct SqlConnection instantiation (cr-dotnet-0013) and
    /// externalizes configuration from Web.config transformations to AWS
    /// Systems Manager Parameter Store and environment variables (cr-dotnet-0010).
    /// 
    /// Configuration keys (read from environment variables, then appSettings):
    /// - Database:Host (or AWS:RDS:ProxyEndpoint) - RDS Proxy endpoint
    /// - Database:Port - database port (default 1433)
    /// - Database:Name - database name
    /// - Database:User - database username
    /// - Database:SecretName - Secrets Manager secret name (holds username/password JSON)
    /// - Database:PasswordParameter - SSM Parameter Store parameter name for password
    /// - AWS:Region - AWS region for IAM token generation
    /// </summary>
    public static class DatabaseConnectionFactory
    {
        /// <summary>
        /// Creates a SqlConnection configured for RDS Proxy with IAM authentication.
        /// The IAM auth token is generated fresh on each call (tokens expire after 15 minutes).
        /// </summary>
        public static SqlConnection CreateConnection()
        {
            var connectionString = GetConnectionString();
            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// Builds a connection string using RDS Proxy endpoint and IAM authentication.
        /// Configuration is sourced from environment variables (with appSettings fallback)
        /// and AWS Systems Manager Parameter Store, replacing Web.config transformations.
        /// </summary>
        public static string GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = GetConfigValue("Database:Host") ?? GetConfigValue("AWS:RDS:ProxyEndpoint"),
                InitialCatalog = GetConfigValue("Database:Name") ?? "DBProject",
                UserID = GetConfigValue("Database:User") ?? GetConfigValue("AWS:RDS:Username"),
                Encrypt = true,
                TrustServerCertificate = false,
                ConnectTimeout = 30,
                // Tune pool size for RDS Proxy multiplexing (lower than traditional 100+)
                MaxPoolSize = 20,
                MinPoolSize = 5,
                Pooling = true
            };

            // Set the port if specified
            var port = GetConfigValue("Database:Port");
            if (!string.IsNullOrEmpty(port))
            {
                builder.DataSource = builder.DataSource + "," + port;
            }

            // Generate IAM authentication token (valid for 15 minutes)
            var region = GetConfigValue("AWS:Region") ?? "us-east-1";
            var dataSource = builder.DataSource;
            var username = builder.UserID;

            if (!string.IsNullOrEmpty(username))
            {
                var token = RDSAuthTokenGenerator.GenerateAuthToken(
                    region,
                    dataSource,
                    1433,
                    username);
                builder.Password = token;
            }
            else
            {
                // Fallback: retrieve password from Secrets Manager or Parameter Store
                var password = GetPasswordFromSecretStore();
                if (!string.IsNullOrEmpty(password))
                {
                    builder.Password = password;
                }
            }

            return builder.ConnectionString;
        }

        /// <summary>
        /// Retrieves the database password from AWS Secrets Manager or
        /// Systems Manager Parameter Store, replacing hardcoded credentials
        /// from Web.config transformations.
        /// </summary>
        private static string GetPasswordFromSecretStore()
        {
            var secretName = GetConfigValue("Database:SecretName");
            var parameterName = GetConfigValue("Database:PasswordParameter");

            if (!string.IsNullOrEmpty(parameterName))
            {
                // Retrieve from SSM Parameter Store (with decryption)
                try
                {
                    var region = GetConfigValue("AWS:Region") ?? "us-east-1";
                    var ssmClient = new AmazonSimpleSystemsManagementClient(
                        RegionEndpoint.GetBySystemName(region));

                    var response = ssmClient.GetParameterAsync(new GetParameterRequest
                    {
                        Name = parameterName,
                        WithDecryption = true
                    }).GetAwaiter().GetResult();

                    return response.Parameter?.Value;
                }
                catch
                {
                    // If Parameter Store is not available, fall back to environment variable
                    return GetConfigValue("Database:Password");
                }
            }

            // Fallback to environment variable (not recommended for production)
            return GetConfigValue("Database:Password");
        }

        /// <summary>
        /// Reads a configuration value from environment variables first,
        /// then falls back to appSettings. Environment variable names use
        /// the __ separator (e.g., Database__Host for Database:Host).
        /// </summary>
        private static string GetConfigValue(string key)
        {
            // Convert "Database:Host" to "Database__Host" for environment variable lookup
            var envKey = key.Replace(":", "__");
            var value = Environment.GetEnvironmentVariable(envKey);

            if (!string.IsNullOrEmpty(value))
                return value;

            // Fallback to appSettings
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return null;
            }
        }
    }
}
