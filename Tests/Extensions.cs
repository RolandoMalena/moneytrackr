using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTrackr.Tests
{
    public static class Extensions
    {
        /// <summary>
        /// Extract the response from a HttpContent and attempt to Deserialize it to a target type
        /// </summary>
        /// <typeparam name="T">Target Type you want to Deserialize to</typeparam>
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

        /// <summary>
        /// Serializes an object and inserts it content into a new HttpContent
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>HTTPContent that should be set as the Content of an HttpClient</returns>
        public static HttpContent ToHttpContent(this object obj)
        {
            try
            {
                string json = JsonConvert.SerializeObject(obj);
                return new StringContent(json, Encoding.UTF8, "application/json");
            }
            catch
            {
                return null;
            }
        }
    }
}
