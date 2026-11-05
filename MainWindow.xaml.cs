using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using RepoStudyPP.Models;

namespace RepoStudyPP;

public partial class MainWindow : Window
{
    private User? _currentUser;
    private RequestItem? _selectedRequest;

    public MainWindow()
    {
        InitializeComponent();

        RequestDateBox.Text = DateTime.Today.ToString("dd.MM.yyyy");
        VetDateBox.Text = DateTime.Today.ToString("dd.MM.yyyy");
        NewAnimalDateBox.Text = DateTime.Today.ToString("dd.MM.yyyy");

        SpeciesFilterBox.ItemsSource = new[] { "Все виды", "Собака", "Кошка", "Другое" };
        SpeciesFilterBox.SelectedIndex = 0;
        GenderFilterBox.ItemsSource = new[] { "Любой пол", "Самка", "Самец" };
        GenderFilterBox.SelectedIndex = 0;
        UpdateBreedFilter();

        RefreshAnimalList();
        RefreshRequestList();
        RefreshVeterinaryList();
        ApplyRoleAccess();
        ShowPage(HomePage);
    }

    private bool IsAdmin => _currentUser?.Role == UserRole.Admin;
    private bool IsVolunteer => _currentUser?.Role == UserRole.Volunteer;
    private bool IsClient => _currentUser?.Role == UserRole.Client;
    private bool IsVeterinarian => _currentUser?.Role == UserRole.Veterinarian;

    private void ShowPage(Grid page)
    {
        HomePage.Visibility = Visibility.Collapsed;
        ShelterPage.Visibility = Visibility.Collapsed;
        LoginPage.Visibility = Visibility.Collapsed;
        RegisterPage.Visibility = Visibility.Collapsed;
        AddAnimalPage.Visibility = Visibility.Collapsed;
        RequestsPage.Visibility = Visibility.Collapsed;
        VeterinaryPage.Visibility = Visibility.Collapsed;
        ReportsPage.Visibility = Visibility.Collapsed;

        page.Visibility = Visibility.Visible;
        GlobalMessageText.Text = "";
    }

    private void ApplyRoleAccess()
    {
        CurrentUserText.Text = _currentUser is null
            ? "Гость"
            : $"{_currentUser.FullName} — {GetRoleName(_currentUser.Role)}";

        AccountButton.Content = _currentUser is null ? "Войти" : "Выйти";
        RequestsNavButton.Visibility = _currentUser is null
            ? Visibility.Collapsed : Visibility.Visible;
        VeterinaryNavButton.Visibility = IsVeterinarian
            ? Visibility.Visible : Visibility.Collapsed;
        ReportsNavButton.Visibility = IsAdmin
            ? Visibility.Visible : Visibility.Collapsed;
        AddAnimalNavButton.Visibility = IsAdmin || IsVolunteer
            ? Visibility.Visible : Visibility.Collapsed;

        RequestsNavButton.IsEnabled = _currentUser is not null;
        RefreshRequestList();
    }

    private static string GetRoleName(UserRole role) => role switch
    {
        UserRole.Admin => "Администратор",
        UserRole.Client => "Клиент",
        UserRole.Volunteer => "Волонтёр",
        UserRole.Veterinarian => "Ветеринар",
        _ => "Гость"
    };

    private void HomeNav_Click(object sender, RoutedEventArgs e) => ShowPage(HomePage);

    private void ShelterNav_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatistics();
        UpdateBreedFilter();
        RefreshAnimalList();
        ShowPage(ShelterPage);
    }

    private void RequestsNav_Click(object sender, RoutedEventArgs e)
    {
        if (_currentUser is null)
        {
            GlobalMessageText.Text = "Сначала войдите в систему.";
            ShowPage(LoginPage);
            return;
        }

        RefreshRequestList();
        UpdateRequestFormAccess();
        ShowPage(RequestsPage);
    }

    private void VeterinaryNav_Click(object sender, RoutedEventArgs e)
    {
        if (!IsVeterinarian)
        {
            GlobalMessageText.Text = "Раздел доступен ветеринарному врачу.";
            return;
        }

        RefreshVeterinaryList();
        ShowPage(VeterinaryPage);
    }

    private void ReportsNav_Click(object sender, RoutedEventArgs e)
    {
        if (!IsAdmin)
        {
            GlobalMessageText.Text = "Отчёт доступен только администратору.";
            return;
        }

        UpdateReport();
        ShowPage(ReportsPage);
    }

    private void AddAnimalNav_Click(object sender, RoutedEventArgs e)
    {
        if (!IsAdmin && !IsVolunteer)
        {
            GlobalMessageText.Text = "Добавлять животных могут администратор и волонтёр.";
            return;
        }

        NewAnimalNameBox.Text = "";
        NewAnimalNumberBox.Text = "";
        NewAnimalBreedBox.Text = "";
        NewAnimalColorBox.Text = "";
        NewAnimalAgeBox.Text = "";
        NewAnimalWeightBox.Text = "";
        NewAnimalVaccinationsBox.Text = "";
        NewAnimalPhotoBox.Text = "";
        NewAnimalDateBox.Text = DateTime.Today.ToString("dd.MM.yyyy");
        AddAnimalErrorText.Text = "";
        ShowPage(AddAnimalPage);
    }

    private void AccountButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentUser is null)
        {
            LoginBox.Text = "";
            PasswordBox.Password = "";
            LoginErrorText.Text = "";
            ShowPage(LoginPage);
            return;
        }

        _currentUser = null;
        ApplyRoleAccess();
        ShowPage(HomePage);
    }

    private void RegisterNav_Click(object sender, RoutedEventArgs e)
    {
        RegisterNameBox.Text = "";
        RegisterPhoneBox.Text = "";
        RegisterLoginBox.Text = "";
        RegisterPasswordBox.Password = "";
        RegisterErrorText.Text = "";
        RegisterRoleBox.SelectedIndex = 0;
        ShowPage(RegisterPage);
    }

    private void LoginNav_Click(object sender, RoutedEventArgs e) =>
        ShowPage(LoginPage);

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        RegisterErrorText.Text = "";
        string name = RegisterNameBox.Text.Trim();
        string phone = RegisterPhoneBox.Text.Trim();
        string login = RegisterLoginBox.Text.Trim();
        string password = RegisterPasswordBox.Password;
        string roleText = (RegisterRoleBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Client";

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone)
            || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            RegisterErrorText.Text = "Заполните все поля.";
            return;
        }

        if (UserStore.Users.Any(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase)))
        {
            RegisterErrorText.Text = "Такой логин уже занят.";
            return;
        }

        if (!Enum.TryParse(roleText, out UserRole role))
        {
            RegisterErrorText.Text = "Выберите роль.";
            return;
        }

        var user = new User
        {
            FullName = name,
            Phone = phone,
            Login = login,
            Password = password,
            Role = role
        };

        UserStore.Users.Add(user);
        _currentUser = user;
        ApplyRoleAccess();
        ShowPage(ShelterPage);
        UpdateStatistics();
        RefreshAnimalList();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string login = LoginBox.Text.Trim();
        string password = PasswordBox.Password;

        var user = UserStore.Users.FirstOrDefault(item =>
            item.Login.Equals(login, StringComparison.OrdinalIgnoreCase) &&
            item.Password == password);

        if (user is null)
        {
            LoginErrorText.Text = "Неверный логин или пароль.";
            return;
        }

        _currentUser = user;
        LoginErrorText.Text = "";
        ApplyRoleAccess();
        ShowPage(ShelterPage);
        UpdateStatistics();
        RefreshAnimalList();
    }

    private void UpdateStatistics()
    {
        AnimalsCountText.Text = AnimalStore.Animals.Count.ToString();
        AdoptedCountText.Text = AnimalStore.AdoptedCount.ToString();
        TreatmentCountText.Text = AnimalStore.Animals.Count(a => a.Status == "На лечении").ToString();
        WaitingCountText.Text = AnimalStore.Animals.Count(a => a.Status == "Ожидает семью").ToString();
    }

    private void AnimalSearchBox_TextChanged(object sender, TextChangedEventArgs e) =>
        RefreshAnimalList();

    private void AnimalFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        RefreshAnimalList();

    private void UpdateBreedFilter()
    {
        string selected = BreedFilterBox?.SelectedItem?.ToString() ?? "Все породы";
        var breeds = new[] { "Все породы" }.Concat(
            AnimalStore.Animals.Select(a => a.Breed).Distinct().OrderBy(b => b)).ToList();
        BreedFilterBox.ItemsSource = breeds;
        BreedFilterBox.SelectedItem = breeds.Contains(selected) ? selected : "Все породы";
    }

    private void RefreshAnimalList()
    {
        if (AnimalsList is null)
            return;

        string search = AnimalSearchBox?.Text?.Trim() ?? "";
        string species = SpeciesFilterBox?.SelectedItem?.ToString() ?? "Все виды";
        string gender = GenderFilterBox?.SelectedItem?.ToString() ?? "Любой пол";
        string breed = BreedFilterBox?.SelectedItem?.ToString() ?? "Все породы";

        var animals = AnimalStore.Animals
            .Where(a => string.IsNullOrWhiteSpace(search)
                || a.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || a.Number.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Where(a => species == "Все виды" || a.Species == species)
            .Where(a => gender == "Любой пол" || a.Gender == gender)
            .Where(a => breed == "Все породы" || a.Breed == breed)
            .ToList();

        AnimalsList.ItemsSource = animals;
        AnimalCountLabel.Text = $"Найдено: {animals.Count}";
    }

    private void AnimalsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AnimalsList.SelectedItem is not AnimalItem animal)
            return;

        AnimalDetailsText.Text =
            $"Кличка: {animal.Name}\n" +
            $"Номер: {animal.Number}\n" +
            $"Вид: {animal.Species}\n" +
            $"Порода: {animal.Breed}\n" +
            $"Пол: {animal.Gender}\n" +
            $"Окрас: {animal.Color}\n" +
            $"Размер: {animal.Size}\n" +
            $"Возраст: {animal.Age}\n" +
            $"Вес: {animal.Weight}\n" +
            $"Прививки: {animal.Vaccinations}\n" +
            $"Фото: {animal.Photo}\n" +
            $"Дата поступления: {animal.ArrivalDate}\n" +
            $"Статус: {animal.Status}";
    }

    private void SaveAnimalButton_Click(object sender, RoutedEventArgs e)
    {
        AddAnimalErrorText.Text = "";

        if (!IsAdmin && !IsVolunteer)
        {
            AddAnimalErrorText.Text = "Нет прав для добавления животных.";
            return;
        }

        string name = NewAnimalNameBox.Text.Trim();
        string number = NewAnimalNumberBox.Text.Trim();
        string species = (NewAnimalSpeciesBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Собака";
        string gender = (NewAnimalGenderBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Самка";
        string size = (NewAnimalSizeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Средний";
        string breed = NewAnimalBreedBox.Text.Trim();
        string color = NewAnimalColorBox.Text.Trim();
        string age = NewAnimalAgeBox.Text.Trim();
        string weight = NewAnimalWeightBox.Text.Trim();
        string vaccinations = NewAnimalVaccinationsBox.Text.Trim();
        string photo = NewAnimalPhotoBox.Text.Trim();
        string arrivalDateText = NewAnimalDateBox.Text.Trim();

        if (!DateTime.TryParseExact(arrivalDateText, "dd.MM.yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var arrivalDate))
        {
            AddAnimalErrorText.Text = "Введите дату поступления в формате ДД.ММ.ГГГГ.";
            return;
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(number))
        {
            AddAnimalErrorText.Text = "Заполните кличку и номер животного.";
            return;
        }

        if (AnimalStore.Animals.Any(a =>
            a.Number.Equals(number, StringComparison.OrdinalIgnoreCase)))
        {
            AddAnimalErrorText.Text = "Животное с таким номером уже есть в списке.";
            return;
        }

        AnimalStore.Animals.Add(new AnimalItem(
            name, number, species,
            string.IsNullOrWhiteSpace(breed) ? "Не указана" : breed,
            gender,
            string.IsNullOrWhiteSpace(color) ? "Не указан" : color,
            size,
            string.IsNullOrWhiteSpace(age) ? "Не указан" : age,
            string.IsNullOrWhiteSpace(weight) ? "Не указан" : weight,
            string.IsNullOrWhiteSpace(vaccinations) ? "Не указаны" : vaccinations,
            "В приюте",
            string.IsNullOrWhiteSpace(photo) ? "Не прикреплено" : photo,
            arrivalDate.ToString("dd.MM.yyyy")));

        UpdateStatistics();
        UpdateBreedFilter();
        RefreshAnimalList();
        ShowPage(ShelterPage);
        GlobalMessageText.Text = $"Животное «{name}» добавлено.";
    }

    private void RequestTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AdoptionRequestFields is null || SurrenderRequestFields is null)
            return;

        bool adoption = (RequestTypeBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "Adoption";
        AdoptionRequestFields.Visibility = adoption ? Visibility.Visible : Visibility.Collapsed;
        SurrenderRequestFields.Visibility = adoption ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateRequestFormAccess()
    {
        bool canCreate = IsClient;
        RequestFormCard.Visibility = canCreate ? Visibility.Visible : Visibility.Collapsed;
        RequestAnimalBox.ItemsSource = AnimalStore.Animals
            .Where(a => a.Status != "На лечении")
            .ToList();
        RequestAnimalBox.SelectedItem = null;
    }

    private void RefreshRequestList()
    {
        if (RequestsList is null)
            return;

        IEnumerable<RequestItem> requests = RequestStore.Requests;

        if (IsClient && _currentUser is not null)
        {
            requests = requests.Where(r =>
                r.ClientName.Equals(_currentUser.FullName, StringComparison.OrdinalIgnoreCase) ||
                r.Phone.Equals(_currentUser.Phone, StringComparison.OrdinalIgnoreCase));
        }

        var list = requests.OrderByDescending(r => r.Date)
                           .ThenByDescending(r => r.Id)
                           .ToList();

        RequestsList.ItemsSource = list;
        RequestsCountLabel.Text = $"Всего заявок: {list.Count}";
        _selectedRequest = null;
        ProcessRequestButton.Visibility = Visibility.Collapsed;
    }

    private void CreateRequestButton_Click(object sender, RoutedEventArgs e)
    {
        RequestErrorText.Text = "";

        if (!IsClient || _currentUser is null)
        {
            RequestErrorText.Text = "Заявку может оформить только клиент.";
            return;
        }

        bool adoption = (RequestTypeBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "Adoption";
        string animalNumber = "";
        string animalName = "";
        string description = "";

        if (adoption)
        {
            if (RequestAnimalBox.SelectedItem is not AnimalItem animal)
            {
                RequestErrorText.Text = "Выберите животное.";
                return;
            }

            animalNumber = animal.Number;
            animalName = animal.Name;
        }
        else
        {
            description = SurrenderDescriptionBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                RequestErrorText.Text = "Опишите животное, которое хотите передать в приют.";
                return;
            }
        }

        if (!DateTime.TryParseExact(RequestDateBox.Text.Trim(), "dd.MM.yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            RequestErrorText.Text = "Введите дату в формате ДД.ММ.ГГГГ.";
            return;
        }

        int id = RequestStore.Requests.Count == 0
            ? 1 : RequestStore.Requests.Max(r => r.Id) + 1;

        RequestStore.Requests.Add(new RequestItem
        {
            Id = id,
            ClientName = _currentUser.FullName,
            Phone = _currentUser.Phone,
            TypeValue = adoption ? RequestType.Adoption : RequestType.Surrender,
            AnimalNumber = animalNumber,
            AnimalName = animalName,
            AnimalDescription = description,
            Date = date
        });

        SurrenderDescriptionBox.Text = "";
        RequestAnimalBox.SelectedItem = null;
        RefreshRequestList();
        GlobalMessageText.Text = $"Заявка №{id} оформлена.";
    }

    private void RequestsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedRequest = RequestsList.SelectedItem as RequestItem;

        bool canProcess = _selectedRequest is not null
            && (IsAdmin || IsVolunteer)
            && _selectedRequest.TypeValue == RequestType.Adoption
            && _selectedRequest.Status == "Новая";

        ProcessRequestButton.Visibility = canProcess
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ProcessRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRequest is null || (!IsAdmin && !IsVolunteer))
            return;

        var animal = AnimalStore.Animals.FirstOrDefault(a =>
            a.Number.Equals(_selectedRequest.AnimalNumber, StringComparison.OrdinalIgnoreCase));

        if (animal is null)
        {
            GlobalMessageText.Text = "Животное из заявки не найдено.";
            return;
        }

        if (animal.Status == "На лечении")
        {
            GlobalMessageText.Text = "Нельзя оформить выдачу животного на лечении.";
            return;
        }

        AnimalStore.Animals.Remove(animal);
        AnimalStore.AdoptedCount++;
        _selectedRequest.Status = "Выдано в семью";

        RefreshRequestList();
        UpdateStatistics();
        RefreshAnimalList();
        GlobalMessageText.Text = $"Животное «{animal.Name}» выдано в семью.";
    }

    private void RefreshVeterinaryList()
    {
        if (VetRecordsList is null)
            return;

        VetRecordsList.ItemsSource = VeterinaryStore.Records
            .OrderByDescending(r => r.Date)
            .ToList();

        VetRecordsCountLabel.Text = $"Всего записей: {VeterinaryStore.Records.Count}";

        var animals = AnimalStore.Animals.ToList();
        VetAnimalBox.ItemsSource = animals;
        if (VetAnimalBox.SelectedItem is null && animals.Count > 0)
            VetAnimalBox.SelectedIndex = 0;
    }

    private void SaveVetButton_Click(object sender, RoutedEventArgs e)
    {
        VetErrorText.Text = "";

        if (!IsVeterinarian)
        {
            VetErrorText.Text = "Добавлять записи может только ветеринар.";
            return;
        }

        if (VetAnimalBox.SelectedItem is not AnimalItem animal)
        {
            VetErrorText.Text = "Выберите животное.";
            return;
        }

        string procedure = VetProcedureBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(procedure))
        {
            VetErrorText.Text = "Укажите процедуру.";
            return;
        }

        if (!DateTime.TryParseExact(VetDateBox.Text.Trim(), "dd.MM.yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            VetErrorText.Text = "Введите дату в формате ДД.ММ.ГГГГ.";
            return;
        }

        string notes = VetNotesBox.Text.Trim();
        VeterinaryStore.Records.Add(new VeterinaryItem(
            animal.Name, animal.Number, procedure, date,
            string.IsNullOrWhiteSpace(notes) ? "Без примечания" : notes));

        VetProcedureBox.Text = "";
        VetNotesBox.Text = "";
        VetDateBox.Text = DateTime.Today.ToString("dd.MM.yyyy");
        RefreshVeterinaryList();
        GlobalMessageText.Text = "Ветеринарная запись сохранена.";
    }

    private void UpdateReport()
    {
        ReportAnimalsText.Text = $"Животных в приюте: {AnimalStore.Animals.Count}";
        ReportAdoptedText.Text = $"Отданы в семьи: {AnimalStore.AdoptedCount}";
        ReportTreatmentText.Text = $"На лечении: {AnimalStore.Animals.Count(a => a.Status == "На лечении")}";
        ReportWaitingText.Text = $"Ожидают семью: {AnimalStore.Animals.Count(a => a.Status == "Ожидает семью")}";
        ReportRequestsText.Text = $"Всего заявок: {RequestStore.Requests.Count}";
        ReportVetText.Text = $"Ветеринарных записей: {VeterinaryStore.Records.Count}";
    }
}
