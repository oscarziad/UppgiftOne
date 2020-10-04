using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerService.Models;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient _client;

        public Worker(ILogger<Worker> logger)
        {

            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _client = new HttpClient();
            _logger.LogInformation("The service has been started.");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {

            _client.Dispose();
            _logger.LogInformation("The service has been stopped.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                var httpClient = HttpClientFactory.Create();

                var url = "https://api.openweathermap.org/data/2.5/onecall?lat=33.441792&lon=-94.037689&exclude=hourly,daily,minutely&appid=62d274c03fa45dffcda1b3b257b696ce&units=metric";
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url);

                try
                {
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        var content = httpResponseMessage.Content;
                        var data = await content.ReadAsAsync<Rootobject>();
                        _logger.LogInformation($"The temprature is: {data} °C");

                        if (data.current.temp > 30)
                        {
                            _logger.LogInformation("The temprature has exceeded the limit.");
                        }
                    }
                }
                catch
                {
                    _logger.LogInformation($"There was an Error: {httpResponseMessage.StatusCode}");
                }

                await Task.Delay(60 * 1000, stoppingToken);
            }
        }

    }
}
