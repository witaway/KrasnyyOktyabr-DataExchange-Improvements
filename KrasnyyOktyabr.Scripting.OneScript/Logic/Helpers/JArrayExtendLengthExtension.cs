using Newtonsoft.Json.Linq;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Helpers;

public static class JArrayExtendLengthExtension
{
    public static void ExtendLength(this JArray array, int length)
    {
        while (array.Count < length)
        {
            array.Add(JValue.CreateNull());
        }
    }
}