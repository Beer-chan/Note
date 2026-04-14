using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Note.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(string email, string password);
        Task<AuthResponse> RegisterAsync(string email, string password, string passwordConfirmation);
        Task<User> GetCurrentUserAsync();
        Task<bool> LogoutAsync();
        bool IsAuthenticated();
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string TokenKey = "auth_token";
        private readonly string _baseUrl = "http://localhost:8000/api/";

        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Восстанавливаем токен если есть
            var token = Preferences.Get(TokenKey, null);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var data = new { email, password };
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl + "login", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<AuthResponse>(responseText, _jsonOptions);
                SaveToken(result.Token);
                return result;
            }

            throw new Exception($"Ошибка входа: {responseText}");
        }

        public async Task<AuthResponse> RegisterAsync(string email, string password, string passwordConfirmation)
        {
            var data = new
            {
                email,
                password,
                password_confirmation = passwordConfirmation
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl + "register", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<AuthResponse>(responseText, _jsonOptions);
                SaveToken(result.Token);
                return result;
            }

            throw new Exception($"Ошибка регистрации: {responseText}");
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var response = await _httpClient.GetAsync(_baseUrl + "user");
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<User>(responseText, _jsonOptions);
            }

            throw new Exception("Не удалось получить пользователя");
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                await _httpClient.PostAsync(_baseUrl + "logout", null);
            }
            catch { }

            ClearToken();
            return true;
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(Preferences.Get(TokenKey, null));
        }

        private void SaveToken(string token)
        {
            Preferences.Set(TokenKey, token);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void ClearToken()
        {
            Preferences.Remove(TokenKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public class AuthResponse
    {
        public string Message { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
        public string TokenType { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}