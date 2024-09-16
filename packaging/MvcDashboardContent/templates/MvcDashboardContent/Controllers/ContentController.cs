using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MyMvcApp.Data;
using MyMvcApp.Data.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyMvcApp.Controllers
{
    public class ContentController : Controller
    {
        #region Construction

        private readonly ContentDbContext context;
        private readonly IMemoryCache cache;
        private readonly ILogger<ContentController> logger;

        private static readonly Dictionary<string, Regex> compiledRedirectRegex = new Dictionary<string, Regex>();

        public ContentController(ContentDbContext context, IMemoryCache cache, ILogger<ContentController> logger)
        {
            this.context = context;
            this.cache = cache;
            this.logger = logger;
        }

        #endregion

        public async Task<IActionResult> Render(string path)
        {
            // Get current path:
            path = "/" + path;

            // Store path in ViewBag:
            ViewBag.Path = path;

            // Get current culture:
            var currentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;

            // Check for redirections first:
            if (!cache.TryGetValue("Content:PathRedirections", out List<PathRedirection>? redirections))
            {
                redirections = context.ContentPathRedirections.AsNoTracking().OrderBy(r => r.Position).ThenBy(r => r.Id).ToList();
                cache.Set("Content:PathRedirections", redirections);
            }

            // Apply first found matching redirection, if any:
            foreach (var redirection in redirections!)
            {
                // If FromPath is not a regular expression:
                if (!redirection.IsRegex)
                {
                    // If match: redirect:
                    if (path.Equals(redirection.FromPath, StringComparison.OrdinalIgnoreCase))
                    {
                        Response.Headers["Location"] = redirection.ToPath;
                        return StatusCode(redirection.StatusCode);
                    }
                }
                else // If FromPath is a regular expression:
                {
                    // Cache compiled version of FromPath regex in cache:
                    if (!compiledRedirectRegex.TryGetValue(redirection.FromPath, out Regex? fromPathRegex))
                    {
                        fromPathRegex = new Regex(redirection.FromPath, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        compiledRedirectRegex[redirection.FromPath] = fromPathRegex;
                    }
                    // Test the FromPath regex:
                    var match = fromPathRegex.Match(path);
                    // If match: redirect:
                    if (match.Success)
                    {
                        Response.Headers["Location"] = match.Result(redirection.ToPath);
                        return StatusCode(redirection.StatusCode);
                    }
                }
            }

            // Apply security:
            var securedPaths = await context.ContentSecuredPaths.Where(p => path.StartsWith(p.Path) && p.Roles != null).ToListAsync();
            if (securedPaths.Any())
            {
                // Check roles:
                foreach (var securedPath in securedPaths)
                {
                    var roles = securedPath.Roles!.Split(',').Select(r => r.Trim()).Where(r => r.Length > 0).ToList();
                    if (roles.Any(r => r == "*"))
                    {
                        if (!this.User.Identity?.IsAuthenticated ?? false) return Forbid();
                    }
                    else
                    {
                        if (!roles.Any(r => this.User.IsInRole(r))) return Forbid();
                    }
                }
            }

            // Retrieve candidate documents:
            var document = await context.ContentPublishedDocuments
                // Where path matches and has viewname:
                .Where(d => d.Path == path && d.ViewName != null)
                // Get the best match for the current UI culture:
                .Where(d => d.Culture == currentUICulture.Name || d.Culture == currentUICulture.TwoLetterISOLanguageName || d.Culture == null)
                .OrderByDescending(d => d.Culture)
                .FirstOrDefaultAsync();
            if (document == null)
            {
                return View("NotFound");
            }

            // Build model:
            var model = new Models.Content.ContentModel() { Document = document };

            // Render view:
            return View(model.Document.ViewName, model);
        }
    }
}
