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
    }

    private void UpdateNotesDisplay()
    {
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
        // Определяем цвет бейджа
        string badgeHex;
        if (note.Type == "Заметка")
            badgeHex = "#4CAF50";
        else if (note.Type == "Напоминание")
            badgeHex = "#FF9800";
        else
            badgeHex = "#9E9E9E";

        // Бейдж типа
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

        // Заголовок
        var titleLabel = new Label
        {
            Text = note.Title,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            VerticalOptions = LayoutOptions.Center
        };

        var header = new HorizontalStackLayout
        {
            Children = { typeBadge, titleLabel },
            Spacing = 10
        };

        // Текст заметки
        var textLabel = new Label
        {
            Text = note.Text,
            FontSize = 12,
            TextColor = Color.FromArgb("#4A4A4A"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        // Дата
        var dateLabel = new Label
        {
            Text = note.Date.ToString("dd.MM.yyyy HH:mm"),
            FontSize = 10,
            TextColor = Color.FromArgb("#757575"),
            HorizontalOptions = LayoutOptions.End
        };

        // Кнопки: создаём отдельно
        var editButton = new Button
        {
            Text = "✏️ Редакт.",
            BackgroundColor = Color.FromArgb("#2196F3"),
            TextColor = Colors.White,
            FontSize = 11,
            Margin = new Thickness(0, 5, 5, 0),
            Padding = new Thickness(8, 4)
        };

        var deleteButton = new Button
        {
            Text = "✕ Удалить",
            BackgroundColor = Color.FromArgb("#F44336"),
            TextColor = Colors.White,
            FontSize = 11,
            Margin = new Thickness(0, 5, 0, 0),
            Padding = new Thickness(8, 4)
        };

        // Привязываем события — ТОЛЬКО ПОСЛЕ создания кнопок
        editButton.Clicked += (_, _) => EditNote(note);
        deleteButton.Clicked += (_, _) => DeleteNote(note);

        // Горизонтальный контейнер для кнопок
        var buttonContainer = new HorizontalStackLayout
        {
            Children = { editButton, deleteButton },
            HorizontalOptions = LayoutOptions.End,
            Spacing = 5
        };

        // Общий стек содержимого
        var contentStack = new VerticalStackLayout
        {
            Children = { header, textLabel, dateLabel, buttonContainer },
            Spacing = 5
        };

        // Обёртка с закруглёнными углами
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
    private void EditNote(NoteItem note)
    {
        TitleEntry.Text = note.Title;
        TextEditor.Text = note.Text;

        allNotes.Remove(note);
        displayedNotes = new List<NoteItem>(allNotes);
        UpdateNotesDisplay();
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