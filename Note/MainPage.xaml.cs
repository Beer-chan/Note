using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Note;

public partial class MainPage : ContentPage
{
    private readonly List<NoteItem> allNotes = new();
    private List<NoteItem> displayedNotes = new();

    public MainPage()
    {
        InitializeComponent();
        UpdateNotesDisplay();
    }

    private async void OnNoteTypeClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string noteType)
        {
            await AddNoteAsync(noteType);
        }
    }

    private async Task AddNoteAsync(string type)
    {
        var title = TitleEntry.Text?.Trim();
        var text = TextEditor.Text?.Trim();

        if (string.IsNullOrEmpty(title))
        {
            await DisplayAlert("Ошибка", "Введите название заметки", "OK");
            TitleEntry.Focus(); // ← Используем Focus(), а не FocusAsync()
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            await DisplayAlert("Ошибка", "Введите текст заметки", "OK");
            TextEditor.Focus(); // ← Аналогично
            return;
        }

        var note = new NoteItem
        {
            Title = title,
            Text = text,
            Type = type,
            Date = DateTime.Now
        };

        allNotes.Add(note);
        displayedNotes = new List<NoteItem>(allNotes);
        UpdateNotesDisplay();

        TitleEntry.Text = "";
        TextEditor.Text = "";

        await DisplayAlert("Успешно", $"Заметка \"{title}\" добавлена как \"{type}\"", "OK");
    }

    private void UpdateNotesDisplay()
    {
        // ❌ БЫЛО: NotesStackLayout.Children(); — это ошибка!
        // ✅ ПРАВИЛЬНО очистить коллекцию Children
        NotesStackLayout.Children.Clear();

        if (displayedNotes.Count == 0)
        {
            var placeholder = new Label
            {
                Text = "Здесь будут ваши заметки..",
                TextColor = Color.FromArgb("#4A4A4A"),
                FontSize = 14,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(5)
            };
            NotesStackLayout.Children.Add(placeholder);
            return;
        }

        foreach (var note in displayedNotes)
        {
            NotesStackLayout.Children.Add(CreateNoteView(note));
        }
    }

    private View CreateNoteView(NoteItem note)
    {
        string badgeHex = note.Type switch
        {
            "Заметка" => "#4CAF50",
            "Напоминание" => "#FF9800",
            "Праздники" => "#E91E63",
            _ => "#9E9E9E"
        };

        var typeBadge = new Frame
        {
            BackgroundColor = Color.FromArgb(badgeHex),
            CornerRadius = 5,
            Padding = new Thickness(8, 3),
            HorizontalOptions = LayoutOptions.Start,
            HasShadow = false
        };
        typeBadge.Content = new Label
        {
            Text = note.Type,
            TextColor = Colors.White,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold
        };

        // ✅ ИСПРАВЛЕНО: TextColor.Black → Colors.Black
        var titleLabel = new Label
        {
            Text = note.Title,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black, // ← было: TextColor.Black → ошибка CS0103
            VerticalOptions = LayoutOptions.Center
        };

        var header = new HorizontalStackLayout
        {
            Children = { typeBadge, titleLabel },
            Spacing = 10
        };

        var textLabel = new Label
        {
            Text = note.Text,
            FontSize = 12,
            TextColor = Color.FromArgb("#4A4A4A"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var dateLabel = new Label
        {
            Text = note.Date.ToString("dd.MM.yyyy HH:mm"),
            FontSize = 10,
            TextColor = Color.FromArgb("#757575"),
            HorizontalOptions = LayoutOptions.End
        };
        var deleteButton = new Button
        {
            Text = "✕ Удалить",
            BackgroundColor = Color.FromArgb("#F44336"),
            TextColor = Colors.White,
            FontSize = 11,
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 5, 0, 0)
        };
        deleteButton.Clicked += (_, _) => DeleteNote(note);

        var contentStack = new VerticalStackLayout
        {
            Children = { header, textLabel, dateLabel, deleteButton },
            Spacing = 5
        };

        var frame = new Frame
        {
            CornerRadius = 8,
            BackgroundColor = Colors.White,
            BorderColor = Color.FromArgb("#CCCCCC"),
            Padding = 10,
            Margin = new Thickness(0, 0, 0, 5),
            Content = contentStack,
            HasShadow = false
        };

        return frame;
    }

    private void DeleteNote(NoteItem note)
    {
        allNotes.Remove(note);
        displayedNotes = new List<NoteItem>(allNotes);
        UpdateNotesDisplay();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.ToLower() ?? "";

        if (string.IsNullOrEmpty(query))
        {
            displayedNotes = new List<NoteItem>(allNotes);
        }
        else
        {
            displayedNotes = allNotes
                .Where(n => n.Title.ToLower().Contains(query) ||
                           n.Text.ToLower().Contains(query) ||
                           n.Type.ToLower().Contains(query))
                .ToList();
        }

        UpdateNotesDisplay();
    }
}

public class NoteItem
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}