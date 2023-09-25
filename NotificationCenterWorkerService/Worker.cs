using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationCenterWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                StartNotificatinCenterOnIIS();

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        private void StartNotificatinCenterOnIIS()
        {
            try
            {

                string notificationCenter = _configuration.GetSection("SiteName").Value;

                using (ServerManager serverManager = new ServerManager())
                {

                    _logger.LogInformation($"{notificationCenter} 3", DateTimeOffset.Now);

                    Site site = serverManager.Sites.FirstOrDefault(s => s.Name == notificationCenter);

                    if (site != null && site.State != ObjectState.Started)
                    {

                        site.Start();
                        RequestNotificationApi();
                        _logger.LogInformation($"{notificationCenter} yeniden baþlatýldý");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Hata: {ex.Message}");
                throw;
            }
        }

        private async void RequestNotificationApi()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiUrl = "http://ntfcntr.wise-dynamic.com/api/NotificationJob/RecurringNotificationJob";

                    // GET isteði yapýn ve yanýtý alýn
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("API Yanýtý: " + responseContent);
                    }
                    else
                    {
                        Console.WriteLine("API Ýsteði Baþarýsýz: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata Oluþtu: " + ex.Message);
                }
            }
        }
    }
}
