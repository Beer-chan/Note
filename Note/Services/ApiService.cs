using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Note.Services
{
    // ========== МОДЕЛИ ==========
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }

    public class AuthResponse
    {
        public string Message { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
        public string TokenType { get; set; }
    }

    public class NoteModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public DateTime? ReminderDate { get; set; }
        [JsonPropertyName("created_at")]
        public string CreatedAtString { get; set; }

        [JsonPropertyName("updated_at")]
        public string UpdatedAtString { get; set; }

        // Игнорируем при сериализации
        [JsonIgnore]
        public DateTime CreatedAt => DateTime.TryParse(CreatedAtString, out var date) ? date : DateTime.Now;

        [JsonIgnore]
        public DateTime UpdatedAt => DateTime.TryParse(UpdatedAtString, out var date) ? date : DateTime.Now;

    }

    public class NotesResponse
    {
        public List<NoteModel> Notes { get; set; } = new();
    }

    public class NoteResponse
    {
        public string Message { get; set; }
        public NoteModel Note { get; set; }
    }

    // ========== СЕРВИС С ОТЛАДКОЙ ==========
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string TokenKey = "auth_token";
        private const string BASE_URL = "http://127.0.0.1:8000/api/";

        public ApiService()
        {
            Debug.WriteLine("========== ApiService СОЗДАН ==========");
            Debug.WriteLine($"BASE_URL = {BASE_URL}");

            // СОЗДАЁМ HttpClient С ОТКЛЮЧЕННЫМ ПРОКСИ
            var handler = new HttpClientHandler
            {
                Proxy = null,
                UseProxy = false
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var token = Preferences.Get(TokenKey, null);
            Debug.WriteLine($"Токен из Preferences: {(string.IsNullOrEmpty(token) ? "ОТСУТСТВУЕТ" : token.Substring(0, Math.Min(20, token.Length)) + "...")}");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private void LogRequest(string method, string url)
        {
            Debug.WriteLine("========== ЗАПРОС ==========");
            Debug.WriteLine($"Метод: {method}");
            Debug.WriteLine($"URL: {url}");
            Debug.WriteLine($"Базовый адрес клиента: {_httpClient.BaseAddress?.ToString() ?? "НЕ УСТАНОВЛЕН"}");
            Debug.WriteLine($"Заголовок Authorization: {_httpClient.DefaultRequestHeaders.Authorization?.Scheme ?? "ОТСУТСТВУЕТ"}");
        }

        private void LogResponse(HttpResponseMessage response, string content)
        {
            Debug.WriteLine("========== ОТВЕТ ==========");
            Debug.WriteLine($"Статус: {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine($"Тело: {(content.Length > 200 ? content.Substring(0, 200) + "..." : content)}");
        }

        // ========== АВТОРИЗАЦИЯ ==========
        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var url = BASE_URL + "login";
            LogRequest("POST", url);

            var data = new { email, password };
            var json = JsonSerializer.Serialize(data);
            Debug.WriteLine($"Тело запроса: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();
                LogResponse(response, responseText);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AuthResponse>(responseText, _jsonOptions);
                    Preferences.Set(TokenKey, result.Token);
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", result.Token);
                    Debug.WriteLine("✅ Вход успешен");
                    return result;
                }

                Debug.WriteLine("❌ Ошибка входа");
                throw new Exception($"Ошибка входа: {responseText}");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"❌ HTTP ОШИБКА: {ex.Message}");
                Debug.WriteLine($"Внутренняя ошибка: {ex.InnerException?.Message}");
                throw new Exception($"Не удалось подключиться к серверу: {ex.Message}");
            }
        }

        public async Task<AuthResponse> RegisterAsync(string email, string password, string passwordConfirmation)
        {
            var url = BASE_URL + "register";
            LogRequest("POST", url);

            var data = new { email, password, password_confirmation = passwordConfirmation };
            var json = JsonSerializer.Serialize(data);
            Debug.WriteLine($"Тело запроса: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();
            LogResponse(response, responseText);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<AuthResponse>(responseText, _jsonOptions);
                Preferences.Set(TokenKey, result.Token);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.Token);
                Debug.WriteLine("✅ Регистрация успешна");
                return result;
            }

            Debug.WriteLine("❌ Ошибка регистрации");
            throw new Exception($"Ошибка регистрации: {responseText}");
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var url = BASE_URL + "user";
            LogRequest("GET", url);

            var response = await _httpClient.GetAsync(url);
            var responseText = await response.Content.ReadAsStringAsync();
            LogResponse(response, responseText);

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("✅ Пользователь получен");
                return JsonSerializer.Deserialize<User>(responseText, _jsonOptions);
            }

            Debug.WriteLine("❌ Ошибка получения пользователя");
            throw new Exception("Не удалось получить пользователя");
        }

        public async Task<bool> LogoutAsync()
        {
            var url = BASE_URL + "logout";
            LogRequest("POST", url);

            try
            {
                await _httpClient.PostAsync(url, null);
            }
            catch { }

            Preferences.Remove(TokenKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Debug.WriteLine("✅ Выход выполнен");
            return true;
        }

        public bool IsAuthenticated()
        {
            var token = Preferences.Get(TokenKey, null);
            Debug.WriteLine($"Проверка авторизации: {(string.IsNullOrEmpty(token) ? "НЕТ" : "ДА")}");
            return !string.IsNullOrEmpty(token);
        }

        // ========== ЗАМЕТКИ ==========
        public async Task<List<NoteModel>> GetNotesAsync()
        {
            var url = BASE_URL + "notes";
            LogRequest("GET", url);

            try
            {
                var response = await _httpClient.GetAsync(url);
                var responseText = await response.Content.ReadAsStringAsync();
                LogResponse(response, responseText);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<NotesResponse>(responseText, _jsonOptions);
                    Debug.WriteLine($"✅ Получено {result?.Notes?.Count ?? 0} заметок");
                    return result?.Notes ?? new List<NoteModel>();
                }

                Debug.WriteLine("❌ Ошибка загрузки заметок");
                throw new Exception($"Ошибка загрузки заметок: {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"❌ HTTP ОШИБКА: {ex.Message}");
                Debug.WriteLine($"Внутренняя ошибка: {ex.InnerException?.Message}");
                throw new Exception($"Не удалось подключиться к серверу: {ex.Message}");
            }
        }

        public async Task<NoteModel> CreateNoteAsync(string title, string text, string type, DateTime? reminderDate)
        {
            var url = BASE_URL + "notes";
            LogRequest("POST", url);

            var data = new Dictionary<string, object>
            {
                ["title"] = title,
                ["text"] = text,
                ["type"] = type
            };

            if (reminderDate.HasValue)
            {
                data["reminder_date"] = reminderDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }

            var json = JsonSerializer.Serialize(data);
            Debug.WriteLine($"Тело запроса: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();
            LogResponse(response, responseText);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<NoteResponse>(responseText, _jsonOptions);
                Debug.WriteLine("✅ Заметка создана");
                return result?.Note ?? throw new Exception("Не удалось создать заметку");
            }

            Debug.WriteLine("❌ Ошибка создания заметки");
            throw new Exception($"Ошибка создания: {response.StatusCode}");
        }

        public async Task<NoteModel> UpdateNoteAsync(int id, string title, string text, string type, DateTime? reminderDate)
        {
            var url = BASE_URL + "notes/" + id;
            LogRequest("PUT", url);

            var data = new Dictionary<string, object>
            {
                ["title"] = title,
                ["text"] = text,
                ["type"] = type
            };

            if (reminderDate.HasValue)
            {
                data["reminder_date"] = reminderDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();
            LogResponse(response, responseText);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<NoteResponse>(responseText, _jsonOptions);
                Debug.WriteLine("✅ Заметка обновлена");
                return result?.Note ?? throw new Exception("Не удалось обновить заметку");
            }

            Debug.WriteLine("❌ Ошибка обновления заметки");
            throw new Exception($"Ошибка обновления: {response.StatusCode}");
        }

        public async Task<bool> DeleteNoteAsync(int id)
        {
            var url = BASE_URL + "notes/" + id;
            LogRequest("DELETE", url);

            var response = await _httpClient.DeleteAsync(url);
            Debug.WriteLine($"Результат удаления: {response.IsSuccessStatusCode}");
            return response.IsSuccessStatusCode;
        }
    }
}