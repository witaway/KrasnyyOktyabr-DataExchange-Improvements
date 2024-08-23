using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using KrasnyyOktyabr.ApplicationNet48.DependencyInjection;
using KrasnyyOktyabr.ApplicationNet48.Modules.API.Filters;

namespace KrasnyyOktyabr.ApplicationNet48;

public class TextMediaTypeFormatter : MediaTypeFormatter
{
    public TextMediaTypeFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    public override bool CanReadType(Type type)
    {
        return type == typeof(string);
    }

    public override bool CanWriteType(Type type)
    {
        return type == typeof(string);
    }

    public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content,
        IFormatterLogger formatterLogger)
    {
        return Task.Factory.StartNew(() =>
        {
            StreamReader reader = new StreamReader(readStream);
            return (object)reader.ReadToEnd();
        });
    }

    public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
        TransportContext transportContext)
    {
        return Task.Factory.StartNew(() =>
        {
            StreamWriter writer = new StreamWriter(writeStream);
            writer.Write(value);
            writer.Flush();
        });
    }
}

public static class WebApiConfig
{
    public static Action<HttpConfiguration> Register(IServiceProvider provider) => (HttpConfiguration config) =>
    {
        config.DependencyResolver = new DependencyResolver(provider);

        config.MapHttpAttributeRoutes();

        config.Filters.Add(new ValidateModelAttribute());
        config.Formatters.Insert(0, new TextMediaTypeFormatter());
    };
}