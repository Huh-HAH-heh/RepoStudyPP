using System.Collections.ObjectModel;
using System.Globalization;

namespace StudyMauiApp;

public partial class VeterinaryPage : ContentPage
{
    private readonly ObservableCollection<VeterinaryItem> _visibleRecords = [];
    private AnimalItem? _selectedAnimal;

    public VeterinaryPage()
    {
        InitializeComponent();

        DateEntry.Text = DateTime.Today.ToString("dd.MM.yyyy");
        LoadAnimals();
        ApplyRoleUi();
        LoadRecords();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadAnimals();
        ApplyRoleUi();
        LoadRecords();
    }

    private void ApplyRoleUi()
    {
        bool isVet = SessionObject.IsVeterenar;
        CreateRecordCard.IsVisible = isVet;

        PageSubtitle.Text = isVet
            ? "Добавление записей о проведённых процедурах."
            : "Просмотр записей о медицинских процедурах.";
    }

    private void LoadAnimals()
    {
        AnimalOptions.Children.Clear();
        _selectedAnimal = null;
        SelectedAnimalLabel.Text = "Животное не выбрано";

        foreach (var animal in AnimalStore.Animals)
        {
            var button = new Button
            {
                Text = animal.DisplayName,
                BackgroundColor = Color.FromArgb("#F7F8F8"),
                TextColor = Color.FromArgb("#252525"),
                BorderColor = Color.FromArgb("#D6D6D6"),
                BorderWidth = 1,
                CornerRadius = 4,
                HeightRequest = 40,
                Padding = new Thickness(10, 6)
            };

            button.Clicked += (_, _) => SelectAnimal(animal, button);
            AnimalOptions.Children.Add(button);
        }

        if (AnimalStore.Animals.Count == 0)
        {
            AnimalOptions.Children.Add(new Label
            {
                Text = "В приюте пока нет животных.",
                TextColor = Color.FromArgb("#666666"),
                FontSize = 12
            });
        }
    }

    private void SelectAnimal(AnimalItem animal, Button selectedButton)
    {
        foreach (var child in AnimalOptions.Children.OfType<Button>())
        {
            child.BackgroundColor = Color.FromArgb("#F7F8F8");
            child.TextColor = Color.FromArgb("#252525");
        }

        selectedButton.BackgroundColor = Color.FromArgb("#456C83");
        selectedButton.TextColor = Colors.White;

        _selectedAnimal = animal;
        SelectedAnimalLabel.Text = $"Выбрано: {animal.Name} ({animal.Number})";
    }

    private void LoadRecords()
    {
        _visibleRecords.Clear();

        foreach (var record in VeterinaryStore.Records.OrderByDescending(r => r.Date))
            _visibleRecords.Add(record);

        RecordsView.ItemsSource = _visibleRecords;
        RecordsCountLabel.Text = $"Всего записей: {_visibleRecords.Count}";
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ValidationLabel.IsVisible = false;

        if (!SessionObject.IsVeterenar)
        {
            await DisplayAlert("Нет доступа", "Добавлять записи может ветеринарный врач.", "ОК");
            return;
        }

        if (_selectedAnimal is null)
        {
            ValidationLabel.Text = "Выберите животное.";
            ValidationLabel.IsVisible = true;
            return;
        }

        string procedure = ProcedureEntry.Text?.Trim() ?? "";
        string notes = NotesEditor.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(procedure))
        {
            ValidationLabel.Text = "Укажите процедуру.";
            ValidationLabel.IsVisible = true;
            return;
        }

        if (!DateTime.TryParseExact(
                DateEntry.Text?.Trim(),
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            ValidationLabel.Text = "Введите дату в формате ДД.ММ.ГГГГ.";
            ValidationLabel.IsVisible = true;
            return;
        }

        VeterinaryStore.Records.Add(
            new VeterinaryItem(
                _selectedAnimal.Name,
                _selectedAnimal.Number,
                procedure,
                date,
                string.IsNullOrWhiteSpace(notes) ? "Без примечания" : notes));

        await DisplayAlert("Готово", "Запись сохранена.", "ОК");

        ProcedureEntry.Text = "";
        NotesEditor.Text = "";
        DateEntry.Text = DateTime.Today.ToString("dd.MM.yyyy");
        LoadRecords();
    }

    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//ShelterMainPage");
}