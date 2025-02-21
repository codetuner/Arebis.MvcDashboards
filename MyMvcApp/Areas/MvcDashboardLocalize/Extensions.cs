using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

#nullable enable

namespace MyMvcApp.Areas.MvcDashboardLocalize
{
    /// <summary>
    /// Local dashboard helper extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns a hash for the (name of the) given user.
        /// The hash is deterministic and returns the same value accross multiple instances (i.e. in a webfarm).
        /// </summary>
        /// <remarks>
        /// The <see cref="String.GetHashCode()"/> method is not deterministic as explained in 
        /// <see href="https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/"/>.
        /// </remarks>
        public static string GetDeterministicHash(this ClaimsPrincipal? user)
        {
            if (user == null || user.Identity == null || user.Identity.Name == null) return "0";

            // Convert user name into bytes:
            var input = Encoding.UTF8.GetBytes(user.Identity.Name);

            // Generate hash:
            var output = SHA256.HashData(input);
            // Alternatively replace by XxHash3 which is faster (https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/#hashing39)
            // but requires the System.IO.Hashing package:
            //output = System.IO.Hashing.XxHash3.Hash(input);

            // Returns hash formatted as string:
            return String.Concat(Array.ConvertAll(output, h => h.ToString("X2")));
        }

        /// <summary>
        /// Whether the current user is an administrator for localization.
        /// An administrator for localization can create and delete domains and create and delete keys and queries.
        /// </summary>
        public static bool IsAdministrator(this ClaimsPrincipal? user)
        {
            return user != null && (user.IsInRole("Administrator") || user.IsInRole("LocalizeAdministrator"));
        }

        /// <summary>
        /// Cultures the current user has read access to.
        /// Returns null if the user has access to all cultures.
        /// </summary>
        public static List<string>? ReadableCultures(this ClaimsPrincipal? user)
        {
            if (user.IsAdministrator())
            {
                return null;
            }
            else if (user != null && user.IsInRole("LocalizeTranslator"))
            {
                return user.Claims.Where(c => c.Type == "ReadableCulture" || c.Type == "WritableCulture")
                    .Select(c => c.Value).SelectMany(v => v.Split(',').Select(s => s.Trim())).ToList();
            }
            else
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Cultures the current user has write access to.
        /// Returns null if the user has access to all cultures.
        /// </summary>
        public static List<string>? WritableCultures(this ClaimsPrincipal? user)
        {
            if (user.IsAdministrator())
            {
                return null;
            }
            else if (user != null && user.IsInRole("LocalizeTranslator"))
            {
                return user.Claims.Where(c => c.Type == "WritableCulture")
                    .Select(c => c.Value).SelectMany(v => v.Split(',').Select(s => s.Trim())).ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
