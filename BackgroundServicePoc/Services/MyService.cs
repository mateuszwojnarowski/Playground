using Newtonsoft.Json.Linq;
using Serilog.Core;

namespace BackgroundServicePoc.Services;

public class MyService : BackgroundService
{
    private readonly ILogger<MyService> _logger;
    private readonly IConfiguration _configuration;

    public MyService(ILogger<MyService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation(_configuration["MyValue"]);
            
            var json = File.ReadAllText("appsettings.json");
            dynamic appSettings = JObject.Parse(json);
            
            appSettings.MyValue = DateTime.Now;
            
            File.WriteAllTextAsync("appsettings.json", appSettings.ToString(), stoppingToken);
        }
    }
}