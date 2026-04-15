using Note.Services;

namespace Note;

public partial class FirstPage : ContentPage
{
    private readonly ApiService _api = new();

    public FirstPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_api.IsAuthenticated())
        {
            await GoToMainPage();
        }
    }

    private async void Login_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ErrorLabel.Text = "Заполните все поля";
            ErrorLabel.IsVisible = true;
            return;
        }

        SetLoading(true);

        try
        {
            await _api.LoginAsync(EmailEntry.Text.Trim(), PasswordEntry.Text);
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

    private async void Register_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    private void SetLoading(bool isLoading)
    {
        LoginButton.IsEnabled = !isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
        if (isLoading) ErrorLabel.IsVisible = false;
    }

    private async Task GoToMainPage()
    {
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}