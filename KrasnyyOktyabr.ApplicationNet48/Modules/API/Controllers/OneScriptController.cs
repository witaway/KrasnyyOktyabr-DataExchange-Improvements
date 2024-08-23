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
public class OneScriptController(IScriptingService scriptingService, ILogger<OneScriptController> logger) : ApiController
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
        var filesCount = HttpContext.Current.Request.Files.Count;
        var scriptFile = HttpContext.Current.Request.Files.Get("script");
        var inputFile  = HttpContext.Current.Request.Files.Get("input");
        
        if (filesCount != 2 || scriptFile is null || inputFile is null)
        {
            return BadRequest("request must contain 2 multipart sections: script, input");
        }
        
        var scriptStream = scriptFile.InputStream;
        var inputStream = inputFile.InputStream;
        
        MemoryStream resultStream = new();
        
        try
        {
            await scriptingService.RunScriptArbitraryAsync(scriptStream, inputStream, resultStream, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "OneScript error");
            return new JsonTransformController.JsonTransformError(exception);
        }

        resultStream.Position = 0;
        StreamReader reader = new(resultStream);
        string result = await reader.ReadToEndAsync();
        
        return Ok(result);
    }
}
