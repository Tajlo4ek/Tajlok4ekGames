using System.Text.Json;

namespace Utils
{
    public class JsonUtils<TObj>
    {
        public static TObj FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<TObj>(json);
            }
            catch (System.Exception)
            {
                return default;
            }
        }

        public static string ToJson(TObj obj, bool humanReadable = false)
        {
            try
            {
                return JsonSerializer.Serialize(obj, typeof(TObj), new JsonSerializerOptions { WriteIndented = humanReadable });
            }
            catch (System.Exception)
            {
                return default;
            }
        }
    }


}
