using System.Collections.ObjectModel;

namespace StudyMauiApp;

public partial class RequestsPage : ContentPage
{
    private readonly ObservableCollection<RequestItem> _visibleRequests = [];
    private RequestType _requestType = RequestType.Adoption;
    private AnimalItem? _selectedAnimal;

    public RequestsPage()
    {
        InitializeComponent();

        RequestDatePicker.Date = DateTime.Today;
        LoadClientData();
        LoadAvailableAnimals();
        ApplyRoleUi();
        UpdateRequestTypeUi();
        LoadRequests();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadClientData();
        LoadAvailableAnimals();
        ApplyRoleUi();
        UpdateRequestTypeUi();
        LoadRequests();
    }

    private void ApplyRoleUi()
    {
        bool isClient = SessionObject.IsClient;
        bool isEmployee = SessionObject.IsAdmin || SessionObject.IsVolonteer;

        CreateRequestCard.IsVisible = isClient;

        if (isClient)
            PageSubtitle.Text = "Оформление заявки на передачу или приём животного.";
        else if (isEmployee)
            PageSubtitle.Text = "Просмотр заявок и оформление выдачи животных.";
        else
            PageSubtitle.Text = "Для работы с заявками войдите в систему.";
    }

    private void LoadClientData()
    {
        var user = SessionObject.CurrentUser;
        ClientNameEntry.Text = user?.FullName ?? "";
        PhoneEntry.Text = user?.Phone ?? "";
    }

    private void LoadAvailableAnimals()
    {
        AnimalOptions.Children.Clear();
        _selectedAnimal = null;
        SelectedAnimalLabel.Text = "Питомец не выбран";

        var animals = AnimalStore.Animals
            .Where(a => a.Status != "На лечении")
            .ToList();

        foreach (var animal in animals)
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

        if (animals.Count == 0)
        {
            AnimalOptions.Children.Add(new Label
            {
                Text = "Нет доступных животных.",
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

    private void OnAdoptionTypeClicked(object? sender, EventArgs e)
    {
        _requestType = RequestType.Adoption;
        UpdateRequestTypeUi();
    }

    private void OnSurrenderTypeClicked(object? sender, EventArgs e)
    {
        _requestType = RequestType.Surrender;
        UpdateRequestTypeUi();
    }

    private void UpdateRequestTypeUi()
    {
        bool adoption = _requestType == RequestType.Adoption;

        AdoptionFields.IsVisible = adoption;
        SurrenderFields.IsVisible = !adoption;

        AdoptionTypeButton.BackgroundColor = adoption
            ? Color.FromArgb("#456C83")
            : Color.FromArgb("#E1E4E5");
        AdoptionTypeButton.TextColor = adoption ? Colors.White : Color.FromArgb("#252525");

        SurrenderTypeButton.BackgroundColor = adoption
            ? Color.FromArgb("#E1E4E5")
            : Color.FromArgb("#456C83");
        SurrenderTypeButton.TextColor = adoption ? Color.FromArgb("#252525") : Colors.White;
    }

    private void LoadRequests()
    {
        _visibleRequests.Clear();

        var user = SessionObject.CurrentUser;
        IEnumerable<RequestItem> requests = RequestStore.Requests;

        if (SessionObject.IsClient && user is not null)
        {
            requests = requests.Where(r =>
                r.ClientName.Equals(user.FullName, StringComparison.OrdinalIgnoreCase) ||
                r.Phone.Equals(user.Phone, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var request in requests
                     .OrderByDescending(r => r.Date)
                     .ThenByDescending(r => r.Id))
        {
            _visibleRequests.Add(request);
        }

        RequestsView.ItemsSource = _visibleRequests;
        RequestsCountLabel.Text = $"Всего заявок: {_visibleRequests.Count}";
    }

    private async void OnCreateRequestClicked(object? sender, EventArgs e)
    {
        ValidationLabel.IsVisible = false;

        if (!SessionObject.IsClient)
        {
            await DisplayAlert("Нет доступа", "Заявку может оформить клиент.", "ОК");
            return;
        }

        var user = SessionObject.CurrentUser;

        if (user is null)
        {
            await DisplayAlert("Ошибка", "Сначала войдите в систему.", "ОК");
            return;
        }

        string animalNumber = "";
        string animalName = "";
        string animalDescription = "";

        if (_requestType == RequestType.Adoption)
        {
            if (_selectedAnimal is null)
            {
                ValidationLabel.Text = "Выберите животное.";
                ValidationLabel.IsVisible = true;
                return;
            }

            animalNumber = _selectedAnimal.Number;
            animalName = _selectedAnimal.Name;
        }
        else
        {
            animalDescription = SurrenderAnimalEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(animalDescription))
            {
                ValidationLabel.Text = "Опишите животное.";
                ValidationLabel.IsVisible = true;
                return;
            }
        }

        int id = RequestStore.Requests.Count == 0
            ? 1
            : RequestStore.Requests.Max(r => r.Id) + 1;

        RequestStore.Requests.Add(
            new RequestItem
            {
                Id = id,
                ClientName = user.FullName,
                Phone = user.Phone,
                TypeValue = _requestType,
                AnimalNumber = animalNumber,
                AnimalName = animalName,
                AnimalDescription = animalDescription,
                Date = RequestDatePicker.Date
            });

        await DisplayAlert("Заявка оформлена", $"Заявка №{id} сохранена.", "ОК");

        SurrenderAnimalEntry.Text = "";
        LoadAvailableAnimals();
        LoadRequests();
    }

    private async void OnProcessClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsAdmin && !SessionObject.IsVolonteer)
        {
            await DisplayAlert("Нет доступа", "Оформить выдачу могут администратор и волонтер.", "ОК");
            return;
        }

        if (sender is not Button button ||
            button.CommandParameter is not RequestItem request)
            return;

        var animal = AnimalStore.Animals.FirstOrDefault(a =>
            a.Number.Equals(request.AnimalNumber, StringComparison.OrdinalIgnoreCase));

        if (animal is null)
        {
            await DisplayAlert("Ошибка", "Животное из заявки не найдено.", "ОК");
            return;
        }

        if (animal.Status == "На лечении")
        {
            await DisplayAlert("Нельзя оформить выдачу", "Животное находится на лечении.", "ОК");
            return;
        }

        bool confirm = await DisplayAlert(
            "Оформление выдачи",
            $"Передать «{animal.Name}» клиенту {request.ClientName}?",
            "Оформить",
            "Отмена");

        if (!confirm)
            return;

        AnimalStore.Animals.Remove(animal);
        AnimalStore.AdoptedCount++;
        request.Status = "Выдано в семью";

        await DisplayAlert("Готово", "Животное передано в семью.", "ОК");
        LoadRequests();
    }

    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//ShelterMainPage");
}