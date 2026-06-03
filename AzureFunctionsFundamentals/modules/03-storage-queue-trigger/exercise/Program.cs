using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StorageQueueExercise;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddSingleton<JobProcessor>();
builder.Build().Run();
