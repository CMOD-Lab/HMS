using System;
using System.IO;
using DBProject.DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 12-factor: configuration comes from environment variables (ASPNETCORE_URLS / PORT, DB_*, ...).
builder.Configuration.AddEnvironmentVariables();

// Runtime configuration from AWS Systems Manager Parameter Store (replaces Web.Debug/Release.config
// build-time transforms). Set AWS_PARAMETER_STORE_PATH (e.g. /clinic/prod/) to enable; parameters such as
// /clinic/prod/ConnectionStrings/sqlCon1 map to ConnectionStrings:sqlCon1. Environment variables still win.
var parameterStorePath = Environment.GetEnvironmentVariable("AWS_PARAMETER_STORE_PATH");
if (!string.IsNullOrEmpty(parameterStorePath))
{
	builder.Configuration.AddSystemsManager(parameterStorePath);
	builder.Configuration.AddEnvironmentVariables();
}

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port) && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
	builder.WebHost.UseUrls("http://0.0.0.0:" + port);
}

// Structured (JSON) console logging for CloudWatch.
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddRazorPages(o =>
{
	// Keep legacy Web Forms URLs (/SignUp.aspx) working for existing links and redirects.
	o.Conventions.AddPageRoute("/SignUp", "SignUp.aspx");
	o.Conventions.AddPageRoute("/SignUp", "");
});

// Session state must be externalised so the app is stateless / horizontally scalable.
// Set REDIS_CONNECTION (Amazon ElastiCache for Redis) to share sessions between instances.
var redis = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrEmpty(redis))
{
	builder.Services.AddStackExchangeRedisCache(o =>
	{
		o.Configuration = redis;
		o.InstanceName = "clinic-session:";
	});

	// Share Data Protection keys (session cookie / antiforgery) across all instances via Redis.
	var mux = ConnectionMultiplexer.Connect(redis);
	builder.Services.AddDataProtection()
		.SetApplicationName("ClinicManagementSystem")
		.PersistKeysToStackExchangeRedis(mux, "clinic-dataprotection-keys");
}
else
{
	builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddSession(o =>
{
	o.Cookie.HttpOnly = true;
	o.Cookie.IsEssential = true;
	o.IdleTimeout = TimeSpan.FromMinutes(20);
});

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
	o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	o.KnownNetworks.Clear();
	o.KnownProxies.Clear();
});

// Local-development fallback connection string for the DAL (env vars take precedence, see DbConnectionFactory).
DbConnectionFactory.Configure(builder.Configuration.GetConnectionString("sqlCon1"));

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
}

// Existing static assets (css/js/images) are served from /assets.
var assetsPath = Path.Combine(app.Environment.ContentRootPath, "assets");
if (Directory.Exists(assetsPath))
{
	app.UseStaticFiles(new StaticFileOptions
	{
		FileProvider = new PhysicalFileProvider(assetsPath),
		RequestPath = "/assets"
	});
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapRazorPages();

app.Run();
