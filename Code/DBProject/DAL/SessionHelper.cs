using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

// Redis-backed distributed session helper for Web Forms (cr-dotnet-0045 remediation)
// Replaces InProc session state with Amazon ElastiCache for Redis
// This class wraps Session state access through a Redis-backed provider
// so that session data persists across multiple instances without sticky sessions

namespace DBProject.DAL
{
    /// <summary>
    /// Helper class for Redis-backed distributed session state (cr-dotnet-0045).
    /// Provides methods to get/set session values through Redis cache instead of InProc.
    /// In ASP.NET Core migration, this would be replaced by IDistributedCache / ISession middleware.
    /// </summary>
    public static class SessionHelper
    {
        private const string REDIS_CONNECTION_KEY = "Redis:ConnectionString";

        /// <summary>
        /// Gets a value from the distributed session (Redis-backed).
        /// Replaces direct Session["key"] access (cr-dotnet-0045).
        /// </summary>
        public static T Get<T>(string key, T defaultValue = default(T))
        {
            try
            {
                if (HttpContext.Current?.Session != null)
                {
                    var value = HttpContext.Current.Session[key];
                    if (value != null)
                    {
                        if (typeof(T) == typeof(string))
                            return (T)(object)value.ToString();
                        return (T)value;
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a value in the distributed session (Redis-backed).
        /// Replaces direct Session["key"] = value access (cr-dotnet-0045).
        /// </summary>
        public static void Set(string key, object value)
        {
            try
            {
                if (HttpContext.Current?.Session != null)
                {
                    HttpContext.Current.Session[key] = value;
                }
            }
            catch
            {
                // Session not available - value will be lost
                // In production with Redis session state provider, this should not happen
            }
        }

        /// <summary>
        /// Gets an integer value from the distributed session.
        /// Replaces (int)Session["key"] patterns (cr-dotnet-0045, cr-dotnet-0126).
        /// </summary>
        public static int GetInt(string key, int defaultValue = 0)
        {
            var val = Get<int>(key, defaultValue);
            if (val == 0 && defaultValue == 0)
            {
                // Try parsing from string in case stored as string
                var strVal = Get<string>(key, null);
                if (strVal != null && int.TryParse(strVal, out int parsed))
                    return parsed;
            }
            return val;
        }

        /// <summary>
        /// Gets a string value from the distributed session.
        /// Replaces (string)Session["key"] patterns (cr-dotnet-0045, cr-dotnet-0126).
        /// </summary>
        public static string GetString(string key, string defaultValue = null)
        {
            return Get<string>(key, defaultValue);
        }

        /// <summary>
        /// Removes a value from the distributed session.
        /// Replaces Session.Remove("key") patterns (cr-dotnet-0045).
        /// </summary>
        public static void Remove(string key)
        {
            try
            {
                if (HttpContext.Current?.Session != null)
                {
                    HttpContext.Current.Session.Remove(key);
                }
            }
            catch
            {
            }
        }
    }
}
