using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace MoneyTrackr.Tests
{
    public static class Extensions
    {
        public static async Task<T> Deserialize<T>(this HttpContent content)
        {
            string json = await content.ReadAsStringAsync();

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return default(T);
            }
        }
    }
}
