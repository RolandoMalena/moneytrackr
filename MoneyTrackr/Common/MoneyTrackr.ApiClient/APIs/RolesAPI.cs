using MoneyTrackr.Shared.DTOs;
using System.Net.Http.Json;

namespace MoneyTrackr.ApiClient.APIs
{
    public interface IRolesAPI
    {
        Task<IList<RoleDto>> GetRolesAsync();
    }

    public class RolesAPI : IRolesAPI
    {
        private readonly HttpClient _httpClient;

        public RolesAPI(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IList<RoleDto>> GetRolesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<RoleDto>>("v1/roles") ?? new RoleDto[0];
        }
    }
}
