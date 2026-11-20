using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// Content Security Policy and security headers HttpModule (cr-dotnet-1041 remediation)
// Adds CSP headers to all HTTP responses to prevent XSS, data injection, and other attacks
// Works alongside AWS WAF for defense-in-depth security

namespace DBProject
{
    /// <summary>
    /// HttpModule that adds Content Security Policy and other security headers
    /// to all HTTP responses (cr-dotnet-1041 remediation).
    /// </summary>
    public class SecurityHeadersModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var response = app.Response;

            if (response == null) return;

            // Content-Security-Policy header (cr-dotnet-1041)
            // Restricts resource loading to trusted sources only
            response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://ajax.googleapis.com https://maxcdn.bootstrapcdn.com; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://maxcdn.bootstrapcdn.com; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' https://fonts.googleapis.com https://maxcdn.bootstrapcdn.com; " +
                "connect-src 'self'; " +
                "frame-ancestors 'self'; " +
                "base-uri 'self'; " +
                "form-action 'self'";

            // Additional security headers
            response.Headers["X-Content-Type-Options"] = "nosniff";
            response.Headers["X-Frame-Options"] = "SAMEORIGIN";
            response.Headers["X-XSS-Protection"] = "1; mode=block";
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        public void Dispose()
        {
            // No resources to dispose
        }
    }
}
