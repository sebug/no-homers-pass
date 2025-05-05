using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Sebug.Function.Models;

namespace Sebug.Function
{
    public class InsertLogTrigger
    {
        private readonly ILogger<InsertLogTrigger> _logger;

        public InsertLogTrigger(ILogger<InsertLogTrigger> logger)
        {
            _logger = logger;
        }

        [Function("InsertLogTrigger")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogError("Receiving logs");
            var sr = new StreamReader(req.Body);
            string logEntriesString = await sr.ReadToEndAsync();
            var logEntries = JsonSerializer.Deserialize<LogEntries>(logEntriesString);
            if (logEntries != null && logEntries.logs != null)
            {
                foreach (var log in logEntries.logs)
                {
                    _logger.LogError("Log from Apple: " + log);
                }
            }
            return new OkObjectResult("Log entries stored");
        }
    }
}
