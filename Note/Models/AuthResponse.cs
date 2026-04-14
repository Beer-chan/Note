using System.Text.Json.Serialization;

namespace YourMauiApp.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}