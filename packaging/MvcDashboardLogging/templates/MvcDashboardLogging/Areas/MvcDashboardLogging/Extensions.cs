using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyMvcApp.Areas.MvcDashboardLogging
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
    }
}
