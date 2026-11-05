using System.Collections.ObjectModel;

namespace StudyMauiApp;

public partial class ShelterMainPage : ContentPage
{
    private readonly ObservableCollection<AnimalItem> _filteredAnimals = [];

    public ShelterMainPage()
    {
        InitializeComponent();

        LoginButton.Clicked += OnLoginClicked;
        ApplyRoleUi();
        ApplyFilters();
        UpdateStats();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ApplyRoleUi();
        ApplyFilters();
        UpdateStats();
    }

    private void ApplyRoleUi()
    {
        var user = SessionObject.CurrentUser;

        CurrentUserLabel.Text = user?.FullName ?? "";
        CurrentRoleLabel.Text = user is null ? "" : GetRoleName(user.Role);

        CurrentUserLabel.IsVisible = user is not null;
        CurrentRoleLabel.IsVisible = user is not null;
        LoginButton.Text = user is null ? "Войти" : "Сменить пользователя";

        AddAnimalButton.IsVisible = SessionObject.IsAdmin || SessionObject.IsVolonteer;
        RequestButton.IsVisible = SessionObject.IsAdmin || SessionObject.IsVolonteer || SessionObject.IsClient;
        VetButton.IsVisible = SessionObject.IsVeterenar;

        ReportsNavButton.IsVisible = SessionObject.IsAdmin;
        RequestsNavButton.IsVisible = SessionObject.IsAdmin || SessionObject.IsVolonteer || SessionObject.IsClient;
    }

    private static string GetRoleName(UserRole role) => role switch
    {
        UserRole.Admin => "Администратор",
        UserRole.Client => "Клиент",
        UserRole.Volonteer => "Волонтер",
        UserRole.Veterenar => "Ветеринарный врач",
        _ => "Гость"
    };

    private void UpdateStats()
    {
        AnimalsCountLabel.Text = AnimalStore.Animals.Count.ToString();
        AdoptedCountLabel.Text = AnimalStore.AdoptedCount.ToString();
        TreatmentCountLabel.Text = AnimalStore.Animals.Count(a => a.Status == "На лечении").ToString();
        WaitingCountLabel.Text = AnimalStore.Animals.Count(a => a.Status == "Ожидает семью").ToString();
    }

    private async void OnLoginClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("LoginPage");

    private void OnFilterChanged(object? sender, EventArgs e) => ApplyFilters();

    private void ApplyFilters()
    {
        _filteredAnimals.Clear();
        string search = SearchBox?.Text?.Trim() ?? "";

        foreach (var animal in AnimalStore.Animals)
        {
            bool matches =
                string.IsNullOrWhiteSpace(search) ||
                animal.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                animal.Number.Contains(search, StringComparison.OrdinalIgnoreCase);

            if (matches)
                _filteredAnimals.Add(animal);
        }

        AnimalsView.ItemsSource = _filteredAnimals;
        ResultsLabel.Text = $"Найдено: {_filteredAnimals.Count}";
    }

    private async void OnAddAnimalClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsAdmin && !SessionObject.IsVolonteer)
        {
            await DisplayAlert("Нет доступа", "Добавлять животных могут администратор и волонтер.", "ОК");
            return;
        }

        await Shell.Current.GoToAsync("AddAnimalPage");
    }

    private async void OnAddRequestClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsAdmin && !SessionObject.IsVolonteer && !SessionObject.IsClient)
        {
            await DisplayAlert("Нет доступа", "Войдите в систему, чтобы оформить заявку.", "ОК");
            return;
        }

        await Shell.Current.GoToAsync("RequestsPage");
    }

    private async void OnVetClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsVeterenar)
        {
            await DisplayAlert("Нет доступа", "Раздел доступен ветеринарному врачу.", "ОК");
            return;
        }

        await Shell.Current.GoToAsync("VeterinaryPage");
    }

    private async void OnReportsClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsAdmin)
        {
            await DisplayAlert("Нет доступа", "Отчёт доступен только администратору.", "ОК");
            return;
        }

        await DisplayAlert(
            "Отчёт",
            $"Животных в приюте: {AnimalStore.Animals.Count}\nОтданы в семьи: {AnimalStore.AdoptedCount}",
            "ОК");
    }

    private async void OnAnimalSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AnimalItem animal)
        {
            await DisplayAlert(
                animal.Name,
                $"Номер: {animal.Number}\n" +
                $"Вид: {animal.Species}\n" +
                $"Порода: {animal.Breed}\n" +
                $"Пол: {animal.Gender}\n" +
                $"Возраст: {animal.Age}\n" +
                $"Вес: {animal.Weight}\n" +
                $"Статус: {animal.Status}",
                "ОК");

            AnimalsView.SelectedItem = null;
        }
    }

    private async void OnHomeClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//ShelterMainPage");

    private async void OnRequestsClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsAdmin && !SessionObject.IsVolonteer && !SessionObject.IsClient)
        {
            await DisplayAlert("Нет доступа", "Сначала войдите в систему.", "ОК");
            return;
        }

        await Shell.Current.GoToAsync("RequestsPage");
    }
}