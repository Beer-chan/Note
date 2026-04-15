using Note.Services;

namespace Note;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _api = new();

    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void Register_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            ErrorLabel.Text = "Введите email";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (!EmailEntry.Text.Contains("@") || !EmailEntry.Text.Contains("."))
        {
            ErrorLabel.Text = "Введите корректный email";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ErrorLabel.Text = "Введите пароль";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (PasswordEntry.Text.Length < 8)
        {
            ErrorLabel.Text = "Пароль должен содержать минимум 8 символов";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ErrorLabel.Text = "Пароли не совпадают";
            ErrorLabel.IsVisible = true;
            return;
        }

        SetLoading(true);

        try
        {
            await _api.RegisterAsync(EmailEntry.Text.Trim(), PasswordEntry.Text, ConfirmPasswordEntry.Text);
            await DisplayAlert("Успех", "Регистрация выполнена успешно!", "OK");
            await GoToMainPage();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
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

    private void SetLoading(bool isLoading)
    {
        RegisterButton.IsEnabled = !isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
        if (isLoading) ErrorLabel.IsVisible = false;
    }

    private async Task GoToMainPage()
    {
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}