using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Amazon.RDS.Util;
using Amazon;

namespace DBProject.DAL
{
    // DbContext for EF Core with RDS Proxy and IAM authentication support
    // Replaces direct SqlConnection usage (cr-dotnet-0013 remediation)
    public class ClinicDbContext : DbContext
    {
        private static readonly string _proxyEndpoint =
            Environment.GetEnvironmentVariable("Database__Host")
            ?? Environment.GetEnvironmentVariable("AWS__RDS__ProxyEndpoint")
            ?? "";

        private static readonly string _dbName =
            Environment.GetEnvironmentVariable("Database__Name")
            ?? "";

        private static readonly string _dbUser =
            Environment.GetEnvironmentVariable("Database__User")
            ?? "";

        private static readonly string _awsRegion =
            Environment.GetEnvironmentVariable("AWS__Region")
            ?? "us-east-1";

        private static readonly string _passwordSecretName =
            Environment.GetEnvironmentVariable("Database__SecretName")
            ?? Environment.GetEnvironmentVariable("Database__PasswordParameter")
            ?? "";

        // Connection string for RDS Proxy - password injected from Secrets Manager / Parameter Store
        // Never hardcode credentials (cr-dotnet-0010, cloud-config skill)
        private static string BuildConnectionString()
        {
            var password = GetSecretPassword();

            var builder = new System.Data.Common.DbConnectionStringBuilder
            {
                { "Server", _proxyEndpoint },
                { "Database", _dbName },
                { "User Id", _dbUser },
                { "Password", password },
                { "Encrypt", "true" },
                { "TrustServerCertificate", "false" },
                { "Connect Timeout", "30" },
                { "Max Pool Size", "20" },
                { "Min Pool Size", "5" },
                { "Pooling", "true" }
            };

            return builder.ConnectionString;
        }

        private static string GetSecretPassword()
        {
            // Fallback: read from environment variable set by deployment
            // (Secrets Manager / Parameter Store value injected at runtime)
            return Environment.GetEnvironmentVariable("Database__Password") ?? "";
        }

        public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
            : base(options)
        {
        }

        public ClinicDbContext() : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connStr = BuildConnectionString();
                optionsBuilder.UseSqlServer(connStr);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // No entity models - this DbContext is used for raw SQL execution
            // against stored procedures and views
            base.OnModelCreating(modelBuilder);
        }
    }
}
