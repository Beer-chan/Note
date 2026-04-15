using Microsoft.Maui.Controls;
using Note.Services;

namespace Note;

public partial class MainPage : ContentPage
{
    private readonly ApiService _api = new();
    private List<NoteModel> allNotes = new();
    private List<NoteModel> displayedNotes = new();
    private NoteModel? _editingNote;

    public MainPage()
    {
        InitializeComponent();
        ReminderDatePicker.Date = DateTime.Now;
        ReminderTimePicker.Time = DateTime.Now.TimeOfDay;
        OnTypeChanged("Заметка");
        LoadUserInfo();
        LoadNotes();
    }

    private void OnTypeChanged(string type)
    {
        bool isReminder = type == "Напоминание";
        ReminderDatePicker.IsEnabled = isReminder;
        ReminderTimePicker.IsEnabled = isReminder;
        BtnNote.BackgroundColor = type == "Заметка" ? Color.FromArgb("#C0C0C0") : Colors.Transparent;
        BtnReminder.BackgroundColor = type == "Напоминание" ? Color.FromArgb("#C0C0C0") : Colors.Transparent;
    }

    private async void LoadUserInfo()
    {
        try
        {
            var user = await _api.GetCurrentUserAsync();
            UserEmailLabel.Text = user.Email;
        }
        catch
        {
            UserEmailLabel.Text = "Не авторизован";
        }
    }

    private async void LoadNotes()
    {
        try
        {
            allNotes = await _api.GetNotesAsync();
            displayedNotes = new List<NoteModel>(allNotes);
            UpdateNotesDisplay();
        }
        catch (Exception ex)
        {
            NotesStackLayout.Children.Clear();
            NotesStackLayout.Children.Add(new Label { Text = $"Ошибка: {ex.Message}", TextColor = Colors.Red });
        }
    }

    private void UpdateNotesDisplay()
    {
        NotesStackLayout.Children.Clear();
        if (displayedNotes.Count == 0)
        {
            NotesStackLayout.Children.Add(new Label { Text = "Нет заметок", TextColor = Colors.Gray });
            return;
        }
        foreach (var note in displayedNotes.OrderByDescending(n => n.CreatedAt))
            NotesStackLayout.Children.Add(CreateNoteView(note));
    }

    private View CreateNoteView(NoteModel note)
    {
        var editBtn = new Button { Text = "✏️", BackgroundColor = Color.FromArgb("#2196F3"), TextColor = Colors.White, WidthRequest = 40 };
        editBtn.Clicked += (_, _) => EditNote(note);

        var delBtn = new Button { Text = "✕", BackgroundColor = Color.FromArgb("#F44336"), TextColor = Colors.White, WidthRequest = 40 };
        delBtn.Clicked += async (_, _) => await DeleteNote(note);

        string dateInfo = note.Type == "Напоминание" && note.ReminderDate.HasValue
            ? $"🔔 {note.ReminderDate.Value:dd.MM.yyyy HH:mm}"
            : note.CreatedAt.ToString("dd.MM.yyyy HH:mm");

        return new Frame
        {
            CornerRadius = 8,
            BackgroundColor = Colors.White,
            BorderColor = Color.FromArgb("#CCCCCC"),
            Padding = 10,
            Margin = new Thickness(0, 0, 0, 5),
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new HorizontalStackLayout
                    {
                        Children =
                        {
                            new Frame { BackgroundColor = Color.FromArgb(note.Type == "Заметка" ? "#4CAF50" : "#FF9800"), CornerRadius = 5, Padding = new Thickness(8,3), Content = new Label { Text = note.Type, TextColor = Colors.White, FontSize = 11 } },
                            new Label { Text = note.Title, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black }
                        },
                        Spacing = 10
                    },
                    new Label { Text = note.Text, FontSize = 12, TextColor = Color.FromArgb("#4A4A4A") },
                    new Label { Text = dateInfo, FontSize = 10, TextColor = Color.FromArgb("#757575"), HorizontalOptions = LayoutOptions.End },
                    new HorizontalStackLayout { Children = { editBtn, delBtn }, HorizontalOptions = LayoutOptions.End, Spacing = 5 }
                },
                Spacing = 5
            }
        };
    }

    private void EditNote(NoteModel note)
    {
        TitleEntry.Text = note.Title;
        TextEditor.Text = note.Text;
        OnTypeChanged(note.Type);
        if (note.ReminderDate.HasValue)
        {
            ReminderDatePicker.Date = note.ReminderDate.Value.Date;
            ReminderTimePicker.Time = note.ReminderDate.Value.TimeOfDay;
        }
        _editingNote = note;
    }

    private async Task DeleteNote(NoteModel note)
    {
        if (!await DisplayAlert("Удаление", "Удалить заметку?", "Да", "Нет")) return;
        try
        {
            await _api.DeleteNoteAsync(note.Id);
            allNotes.Remove(note);
            displayedNotes = new List<NoteModel>(allNotes);
            UpdateNotesDisplay();
            if (_editingNote?.Id == note.Id) ClearForm();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim();
        var text = TextEditor.Text?.Trim();
        var type = ReminderDatePicker.IsEnabled ? "Напоминание" : "Заметка";

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(text))
        {
            await DisplayAlert("Ошибка", "Заполните все поля", "OK");
            return;
        }

        DateTime? reminderDate = type == "Напоминание" ? ReminderDatePicker.Date + ReminderTimePicker.Time : null;

        try
        {
            if (_editingNote != null)
            {
                var updated = await _api.UpdateNoteAsync(_editingNote.Id, title, text, type, reminderDate);
                var idx = allNotes.FindIndex(n => n.Id == _editingNote.Id);
                if (idx >= 0) allNotes[idx] = updated;
                _editingNote = null;
            }
            else
            {
                var created = await _api.CreateNoteAsync(title, text, type, reminderDate);
                allNotes.Insert(0, created);
            }
            displayedNotes = new List<NoteModel>(allNotes);
            UpdateNotesDisplay();
            ClearForm();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e) => ClearForm();

    private void ClearForm()
    {
        TitleEntry.Text = "";
        TextEditor.Text = "";
        _editingNote = null;
        OnTypeChanged("Заметка");
        ReminderDatePicker.Date = DateTime.Now;
        ReminderTimePicker.Time = DateTime.Now.TimeOfDay;
    }

    private void OnNoteTypeClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string type)
            OnTypeChanged(type);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.ToLower() ?? "";
        displayedNotes = string.IsNullOrEmpty(q)
            ? new List<NoteModel>(allNotes)
            : allNotes.Where(n => n.Title.ToLower().Contains(q) || n.Text.ToLower().Contains(q)).ToList();
        UpdateNotesDisplay();
    }

    private async void Logout_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Выход", "Выйти?", "Да", "Нет"))
        {
            await _api.LogoutAsync();
            Application.Current.MainPage = new NavigationPage(new FirstPage());
        }
    }
}