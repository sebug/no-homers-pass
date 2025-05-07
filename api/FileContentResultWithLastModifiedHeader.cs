using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Sebug.Function;

    public class FileContentResultWithLastModifiedHeader : FileContentResult
    {
        public FileContentResultWithLastModifiedHeader(byte[] fileContents, string contentType) : base(fileContents, contentType)
        {
        }

        public FileContentResultWithLastModifiedHeader(byte[] fileContents, MediaTypeHeaderValue contentType) : base(fileContents, contentType)
        {
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (LastModified.HasValue)
            {
                context.HttpContext.Response.Headers["X-Last-Modified"] = LastModified.Value.UtcDateTime.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT";
            }
            await base.ExecuteResultAsync(context);
        }
    }