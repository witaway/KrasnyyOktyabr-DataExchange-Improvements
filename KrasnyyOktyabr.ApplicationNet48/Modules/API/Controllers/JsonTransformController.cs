#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using KrasnyyOktyabr.ApplicationNet48.Modules.API.Attributes;
using KrasnyyOktyabr.Scripting.Core;
using Microsoft.Extensions.Logging;

namespace KrasnyyOktyabr.ApplicationNet48.Modules.API.Controllers;

[ApiRoutePrefix("scripting")]
public class JsonTransformController(IScriptingService scriptingService, ILogger<JsonTransformController> logger) : ApiController
{
    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
    
    [HttpPost]
    [Route("onescript/run", Name = "ExecuteScriptingOneScript")]
    public async Task<IHttpActionResult> Run(CancellationToken cancellationToken)
    {
        var bodyStream = HttpContext.Current.Request.InputStream;
        MemoryStream resultStream = new();

        if (bodyStream is null)
        {
            return BadRequest();
        }

        bodyStream.Position = 0; // Stream has been read and position is in the end now
        
        try
        {
            await scriptingService.RunJsonTransformAsync(bodyStream, resultStream, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Json Transform error");
            return new JsonTransformError(exception);
        }

        resultStream.Position = 0;
        StreamReader reader = new(resultStream);
        string resulbibh = await reader.ReadToEndAsync();

        
        return Ok(resulbibh);
    }
    
    [HttpPost]
    [Route("jsontransform/run", Name = "ExecuteScriptingJsonTransform")]
    public async Task<IHttpActionResult> Run(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        MemoryStream resultStream = new();
    
        Stream bodyStream = await request.Content.ReadAsStreamAsync();
    
        if (bodyStream is null)
        {
            return BadRequest();
        }
    
        bodyStream.Position = 0; // Stream has been read and position is in the end now
    
        try
        {
            await scriptingService.RunJsonTransformAsync(bodyStream, resultStream, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Json Transform error");
            return new JsonTransformError(ex);
        }
    
        response.Content = new JsonTransformContent(resultStream);
    
        return RunResult(response);
    }

    private static JsonTransformRunResult RunResult(HttpResponseMessage response) => new(response);

    public class JsonTransformError(Exception exception) : IHttpActionResult
    {
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(HttpStatusCode.BadRequest);

            IEnumerable<string> stacktrace = exception.StackTrace
                .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());

            response.Content = JsonContent.Create(new Dictionary<string, object>()
            {
                { exception.GetType().Name, exception.Message },
                { "stacktrace", stacktrace },
            });

            return Task.FromResult(response);
        }
    }

    public class JsonTransformRunResult(HttpResponseMessage response) : IHttpActionResult
    {
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }

    public class JsonTransformContent : HttpContent
    {
        private readonly MemoryStream _bodyStream;

        internal JsonTransformContent(MemoryStream bodyStream)
        {
            bodyStream.Capacity = Convert.ToInt32(bodyStream.Length); // Truncate tail of nulls

            _bodyStream = bodyStream;

            Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            await stream.WriteAsync(_bodyStream.GetBuffer(), 0, Convert.ToInt32(_bodyStream.Length)).ConfigureAwait(false);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
    
    public class OneScriptContent : HttpContent
    {
        private readonly MemoryStream _bodyStream;

        internal OneScriptContent(MemoryStream bodyStream)
        {
            bodyStream.Capacity = Convert.ToInt32(bodyStream.Length); // Truncate tail of nulls

            _bodyStream = bodyStream;

            Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = "utf-8" };
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            await stream.WriteAsync(_bodyStream.GetBuffer(), 0, Convert.ToInt32(_bodyStream.Length)).ConfigureAwait(false);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
