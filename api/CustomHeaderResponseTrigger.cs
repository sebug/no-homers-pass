using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Sebug.Function
{
    public class CustomHeaderResponseTrigger
    {
        private readonly ILogger<CustomHeaderResponseTrigger> _logger;

        public CustomHeaderResponseTrigger(ILogger<CustomHeaderResponseTrigger> logger)
        {
            _logger = logger;
        }

        [Function("CustomHeaderResponseTrigger")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.Headers.Add("X-Last-Modified", DateTimeOffset.Now.UtcDateTime.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT");
            response.WriteString("Maybe custom header??" + DateTimeOffset.Now.UtcDateTime.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT");
            return response;
        }
    }
}
