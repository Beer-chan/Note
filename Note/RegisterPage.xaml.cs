using Note.Services;

namespace Note;

public partial class RegisterPage : ContentPage
{
    private readonly IAuthService _authService;

    public RegisterPage()
    {
        InitializeComponent();
        _authService = new AuthService();
    }

    private async void Register_Clicked(object sender, EventArgs e)
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            ShowError("Введите email");
            return;
        }

        if (!EmailEntry.Text.Contains("@") || !EmailEntry.Text.Contains("."))
        {
            ShowError("Введите корректный email");
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowError("Введите пароль");
            return;
        }

        if (PasswordEntry.Text.Length < 8)
        {
            ShowError("Пароль должен содержать минимум 8 символов");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ShowError("Пароли не совпадают");
            return;
        }

        // Блокируем кнопку и показываем загрузку
        SetLoading(true);

        try
        {
            var response = await _authService.RegisterAsync(
                EmailEntry.Text.Trim(),
                PasswordEntry.Text,
                ConfirmPasswordEntry.Text
            );

            await DisplayAlert("Успех", "Регистрация выполнена успешно!", "OK");
            await GoToMainPage();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void BackToLogin_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void SetLoading(bool isLoading)
    {
        RegisterButton.IsEnabled = !isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;

        if (isLoading)
        {
            ErrorLabel.IsVisible = false;
        }
    }

    private async Task GoToMainPage()
    {
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}