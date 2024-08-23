using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KrasnyyOktyabr.ApplicationNet48.Common.Formatters;

public class TextPlainStringFormatter : MediaTypeFormatter
{
    public TextPlainStringFormatter()
    {
        this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
    }

    public override bool CanWriteType(Type type)
    {
        return type == typeof(string);
    }

    public override bool CanReadType(Type type)
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