using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(workerApplication => {
        workerApplication.UseWhen<LastModifiedHttpHeaderMiddleware>((context) => {
            return context.FunctionDefinition.Name == "GetExistingPassTrigger";
        });
    })
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();