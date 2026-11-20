using System;
using System.Collections.Generic;
using System.Web;

namespace DBProject.Security
{
    /// <summary>
    /// HTTP module that adds Content-Security-Policy and other security headers
    /// to every HTTP response. This replaces the need for AWS WAF for basic
    /// CSP enforcement (cr-dotnet-1041).
    /// </summary>
    public class CspHeadersHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var response = app.Response;

            // Content-Security-Policy header
            response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' https://ajax.googleapis.com https://maxcdn.bootstrapcdn.com; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://maxcdn.bootstrapcdn.com; " +
                "img-src 'self' data:; " +
                "font-src 'self' https://fonts.gstatic.com https://maxcdn.bootstrapcdn.com; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none';";

            // Additional security headers
            response.Headers["X-Content-Type-Options"] = "nosniff";
            response.Headers["X-Frame-Options"] = "DENY";
            response.Headers["X-XSS-Protection"] = "1; mode=block";
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
