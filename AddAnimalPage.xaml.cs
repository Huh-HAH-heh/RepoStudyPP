namespace StudyMauiApp;

public partial class AddAnimalPage : ContentPage
{
    public AddAnimalPage()
    {
        InitializeComponent();
        AdmissionDatePicker.Date = DateTime.Today;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ValidationLabel.IsVisible = false;

        if (SessionObject.CurrentUser is not
            { Role: UserRole.Admin or UserRole.Volonteer })
        {
            await DisplayAlert("Нет доступа", "Добавлять животных могут администратор и волонтер.", "ОК");
            return;
        }

        string name = NameEntry.Text?.Trim() ?? "";
        string number = NumberEntry.Text?.Trim() ?? "";
        string species = SpeciesEntry.Text?.Trim() ?? "";
        string gender = GenderEntry.Text?.Trim() ?? "";
        string breed = BreedEntry.Text?.Trim() ?? "";
        string size = SizeEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(number) ||
            string.IsNullOrWhiteSpace(species) ||
            string.IsNullOrWhiteSpace(gender) ||
            string.IsNullOrWhiteSpace(size))
        {
            ValidationLabel.Text = "Заполните все обязательные поля.";
            ValidationLabel.IsVisible = true;
            return;
        }

        if (AnimalStore.Animals.Any(a =>
                a.Number.Equals(number, StringComparison.OrdinalIgnoreCase)))
        {
            ValidationLabel.Text = "Животное с таким номером уже есть в списке.";
            ValidationLabel.IsVisible = true;
            return;
        }

        AnimalStore.Animals.Add(
            new AnimalItem(
                name,
                number,
                species,
                string.IsNullOrWhiteSpace(breed) ? "Не указана" : breed,
                gender,
                "Не указан",
                size,
                "Не указан",
                "Не указан",
                "Не указаны",
                "В приюте",
                "Не прикреплено"));

        await DisplayAlert(
            "Готово",
            $"Животное «{name}» добавлено. Дата поступления: {AdmissionDatePicker.Date:dd.MM.yyyy}.",
            "ОК");

        await Shell.Current.GoToAsync("//ShelterMainPage");
    }

    private async void OnCancelClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//ShelterMainPage");
}