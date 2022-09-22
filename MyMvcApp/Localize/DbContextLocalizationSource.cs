using Arebis.Core.AspNet.Mvc.Localization;
using Arebis.Core.Localization;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MyMvcApp.Data.Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyMvcApp.Localize
{
    /// <summary>
    /// An ILocalizationSource implementation that retrieves the source data for localizing this application from a LocalizeDbContext.
    /// </summary>
    public class DbContextLocalizationSource : ILocalizationSource
    {
        private static readonly Regex argSubstitutionRegex = new("\\{\\{(.*?)(:.*?)?\\}\\}", RegexOptions.Compiled);

        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;
        private readonly LocalizationOptions options;

        public DbContextLocalizationSource(IConfiguration configuration, IServiceProvider serviceProvider, IOptions<LocalizationOptions> localizationOptions)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.options = localizationOptions.Value;
        }

        public LocalizationResourceSet GetResourceSet()
        {
            var result = new LocalizationResourceSet();

            using (var scope = this.serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<LocalizeDbContext>())
            {
                // Load all domains in reverse order:
                foreach (var domainName in options.Domains.Reverse())
                {
                    var domain = context.LocalizeDomains.SingleOrDefault(d => d.Name == domainName);
                    if (domain == null) continue;
                    if (domain.Cultures == null) continue;
                    if (domain.Cultures.Length == 0) continue;

                    // Read cultures:
                    foreach (var culture in domain.Cultures) result.Cultures.Add(culture);
                    result.DefaultCulture ??= domain.Cultures[0];

                    // Create keys from keys:
                    foreach (var key in context.LocalizeKeys.Include(k => k.Values).Where(k => k.DomainId == domain.Id).OrderByDescending(k => k.ForPath!.Length))
                    {
                        // Skip key if none of the values is validated while review is required:
                        if (this.options.UseOnlyReviewedLocalizationValues && !key.Values!.Any(v => v.Reviewed)) continue;

                        // Build and add resource:
                        var resource = new LocalizationResource() { ForPath = key.ForPath };
                        if (result.AddResource(key.Name!, resource, false))
                        {
                            foreach (var value in key.Values!)
                            {
                                // Skip non-reviewed values if review is required:
                                if (this.options.UseOnlyReviewedLocalizationValues && !value.Reviewed) continue;

                                // Substitute named arguments into positional arguments and collect extra substitution fields;
                                // Set resources value for culture:
                                var substf = resource.SubstitutionFields;
                                resource.Values[value.Culture] = SubstituteArgs(value.Value, key.ArgumentNames, ref substf) ?? String.Empty;
                                resource.SubstitutionFields = substf;
                            }
                        }
                    }

                    // Create keys from queries:
                    foreach (var query in context.LocalizeQueries.Where(q => q.DomainId == domain.Id))
                    {
                        using var conn = new SqlConnection(configuration.GetConnectionString(query.ConnectionName));
                        using var cmd = conn.CreateCommand();

                        // Replace "{cultures}" in SQL command (replace single quotes to prevent SQL injection):
                        cmd.CommandText = query.Sql
                            .Replace("{cultures}", String.Join("','", domain.Cultures.Select(c => c.Replace('\'', '*'))));

                        if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                        var queryResources = new Dictionary<string, Dictionary<string, string>>();
                        using (var reader = cmd.ExecuteReader())
                        {
                            var keyCount = (reader.GetColumnSchema().Count - 1) / 2;
                            while (reader.Read())
                            {
                                var culture = reader.GetString(0);
                                if (domain.Cultures.Contains(culture))
                                {
                                    for (int c = 0; c < keyCount; c++)
                                    {
                                        var key = reader.GetString(c * 2 + 1);
                                        var value = reader.GetString(c * 2 + 2);

                                        if (!queryResources.TryGetValue(key, out var dict))
                                        {
                                            dict = queryResources[key] = new Dictionary<string, string>();
                                        }
                                        dict[culture] = value;
                                    }
                                }
                            }
                        }

                        foreach (var pair in queryResources)
                        {
                            result.AddResource(pair.Key, new LocalizationResource() { Values = pair.Value }, false);
                        }
                    }
                }
            }

            // Return result:
            return result;
        }

        private static string? SubstituteArgs(string? str, string[]? argumentNames, ref HashSet<string>? substitutionFields)
        {
            if (String.IsNullOrWhiteSpace(str)) return str;
            var result = str;

            foreach (var match in argSubstitutionRegex.Matches(str).Cast<Match>())
            {
                if (argumentNames != null)
                { 
                    var i = Array.IndexOf(argumentNames, match.Groups[1].Value);
                    if (i >= 0)
                    {
                        result = result.Replace(match.Value, "{" + i + match.Groups[2].Value + "}");
                        continue;
                    }
                }
                substitutionFields ??= new();
                substitutionFields.Add(match.Value);
            }

            return result;
        }
    }
}
