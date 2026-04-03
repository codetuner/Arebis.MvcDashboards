using Arebis.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyMvcApp.Data.Localize;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

namespace MyMvcApp.Localize
{
    /// <summary>
    /// Implementation of a background service for localization jobs.
    /// </summary>
    public class LocalizeBackgroundService : BackgroundService
    {
        private readonly ConcurrentQueue<BackgroundJob> jobsQueue = new();
        private readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<LocalizeBackgroundService> logger;

        public LocalizeBackgroundService(IServiceProvider serviceProvider, ILogger<LocalizeBackgroundService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            Instance = this;
        }

        public static LocalizeBackgroundService? Instance { get; private set; }

        public void EnqueueJob(BackgroundJob job)
        {
            this.jobsQueue.Enqueue(job);
            this.waitHandle.Set();
        }

        ///  <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // Ensure this runs asynchronously
            while (true)
            {
                int index = WaitHandle.WaitAny(new[] { stoppingToken.WaitHandle, this.waitHandle });
                if (index == 0) return; // Stopping token was triggered

                while (this.jobsQueue.TryDequeue(out var job))
                {
                    await ProcessJob(job, stoppingToken);
                    if (stoppingToken.IsCancellationRequested) return; // Check for cancellation after processing each job
                }
            }
        }

        private async Task ProcessJob(BackgroundJob job, CancellationToken stoppingToken)
        {
            try
            {
                using (IServiceScope scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<LocalizeDbContext>();
                    dbContext.Add(job);
                    job.UtcTimeStarted = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();

                    try
                    {
                        if (job.JobType == "AutoTranslateDomain")
                        {
                            await this.AutoTranslateDomain(job, scope, stoppingToken);
                        }

                        job.UtcTimeEnded = DateTime.UtcNow;
                        job.Succeeded = true;
                    }
                    catch (OperationCanceledException)
                    {
                        job.UtcTimeEnded = DateTime.UtcNow;
                        job.Succeeded = false;
                        job.Output += "Job was aborted.";
                    }
                    catch (Exception ex)
                    {
                        job.UtcTimeEnded = DateTime.UtcNow;
                        job.Succeeded = false;
                        job.Output += ex.ToString();
                    }
                    finally
                    {
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process localization job.");
            }
        }

        private async Task AutoTranslateDomain(BackgroundJob job, IServiceScope scope, CancellationToken stoppingToken)
        {
            var context = scope.ServiceProvider.GetRequiredService<LocalizeDbContext>();
            var translationService = scope.ServiceProvider.GetRequiredService<ITranslationService>();

            var parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(job.Parameters ?? "") ?? throw new Exception("Invalid job parameters.");
            var domainId = parameters.ContainsKey("DomainId") ? int.Parse(parameters["DomainId"]) : throw new Exception("DomainId parameter is missing.");
            
            var domain = await context.LocalizeDomains
                .Include(d => d.Keys)
                .Where(d => d.Id == domainId)
                .SingleAsync(stoppingToken);

            var sourceLanguage = domain.Cultures?.FirstOrDefault();
            var targetLanguages = domain.Cultures?.Skip(1).ToArray() ?? [];

            if (sourceLanguage == null || targetLanguages.Length == 0)
            {
                job.Output += "No source or target languages defined for the domain.\r\n";
                return;
            }

            foreach (var key in (domain.Keys ?? []).OrderBy(k => k.Name))
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss} Translating \"${key.Name}\"...");

                if (key.Values == null) await context.Entry(key).Collection(e => e.Values!).LoadAsync(stoppingToken);
                if (key.Values == null) throw new Exception("Key values collection is still null after explicit loading.");

                var sourceValue = key.Values.SingleOrDefault(v => v.Culture == sourceLanguage);
                var sourceText = sourceValue?.Value;
                if (String.IsNullOrEmpty(sourceText)) continue;

                var availableLanguages = key.Values.Where(v => !String.IsNullOrEmpty(v.Value)).Select(v => v.Culture).ToList();
                var toTranslateLanguages = targetLanguages.Except(availableLanguages).ToList();
                if (toTranslateLanguages.Count == 0) continue;

                job.Output += $"Translating key \"{key.Id}\" from {sourceLanguage} to {string.Join(", ", toTranslateLanguages)}...\r\n";

                try
                {
                    var translations = (await translationService.TranslateAsync(sourceLanguage, toTranslateLanguages, key.MimeType ?? "text/html", sourceText, null, stoppingToken))?.ToList();
                    if (translations == null) continue;

                    for (int i = 0; i < translations.Count; i++)
                    {
                        if (String.IsNullOrEmpty(translations[i])) continue;
                        var newValue = key.Values.SingleOrDefault(v => v.Culture == toTranslateLanguages[i]);
                        if (newValue == null)
                        {
                            newValue = new KeyValue();
                            newValue.KeyId = key.Id;
                            newValue.Culture = toTranslateLanguages[i];
                            context.LocalizeKeyValues.Add(newValue);
                        }
                        newValue.Value = translations[i];
                        key.ValuesToReview = (key.ValuesToReview ?? []).Append(toTranslateLanguages[i]).ToArray();
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    job.Output += $"Failed to translate key \"{key.Id}\" to {string.Join(", ", toTranslateLanguages)}: {ex.Message}\r\n";
                }

                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
