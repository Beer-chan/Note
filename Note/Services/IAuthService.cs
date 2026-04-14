using YourMauiApp.Models;

namespace YourMauiApp.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(string email, string password);
        Task<AuthResponse> RegisterAsync(string email, string password, string passwordConfirmation);
        Task<User> GetCurrentUserAsync();
        Task<bool> LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        void SetAuthToken(string token);
        void ClearAuthToken();
    }
}