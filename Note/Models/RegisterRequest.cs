using System.Text.Json.Serialization;

namespace YourMauiApp.Models
{
    public class RegisterRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("password_confirmation")]
        public string PasswordConfirmation { get; set; }
    }
}