namespace RepoStudyPP.Models;

public enum UserRole
{
    Client,
    Admin,
    Volunteer,
    Veterinarian
}

public enum RequestType
{
    Adoption,
    Surrender
}

public sealed class User
{
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public UserRole Role { get; set; }
}

public sealed record AnimalItem(
    string Name,
    string Number,
    string Species,
    string Breed,
    string Gender,
    string Color,
    string Size,
    string Age,
    string Weight,
    string Vaccinations,
    string Status,
    string Photo,
    string ArrivalDate)
{
    public string Details => $"{Species} · {Breed} · {Gender} · {Age} · {Weight}";
    public string DisplayName => $"{Number} — {Name}";
}

public sealed class RequestItem
{
    public int Id { get; init; }
    public string ClientName { get; init; } = "";
    public string Phone { get; init; } = "";
    public RequestType TypeValue { get; init; }
    public string AnimalNumber { get; init; } = "";
    public string AnimalName { get; init; } = "";
    public string AnimalDescription { get; init; } = "";
    public DateTime Date { get; init; }
    public string Status { get; set; } = "Новая";

    public string Type => TypeValue == RequestType.Adoption
        ? "Взятие животного в семью"
        : "Передача животного в приют";

    public string DateText => Date.ToString("dd.MM.yyyy");

    public string AnimalText => TypeValue == RequestType.Adoption
        ? $"Питомец: {AnimalName} ({AnimalNumber})"
        : $"Животное: {AnimalDescription}";
}

public sealed record VeterinaryItem(
    string AnimalName,
    string AnimalNumber,
    string Procedure,
    DateTime Date,
    string Notes)
{
    public string DateText => Date.ToString("dd.MM.yyyy");
    public string AnimalText => $"Питомец: {AnimalName} ({AnimalNumber})";
}

public static class UserStore
{
    public static List<User> Users { get; } =
    [
        new()
        {
            FullName = "Nerd",
            Phone = "+7 666 666-66-66",
            Login = "Nerd",
            Password = "Nerd",
            Role = UserRole.Admin
        },
        new()
        {
            FullName = "GOON",
            Phone = "+7 900 000-00-00",
            Login = "GOON",
            Password = "GOON",
            Role = UserRole.Client
        },
        new()
        {
            FullName = "WERDO",
            Phone = "+7 900 000-00-00",
            Login = "WERDO",
            Password = "WERDO",
            Role = UserRole.Volunteer
        },
        new()
        {
            FullName = "Доктор Иванов",
            Phone = "+7 901 111-11-11",
            Login = "vet",
            Password = "vet",
            Role = UserRole.Veterinarian
        }
    ];
}

public static class AnimalStore
{
    public static List<AnimalItem> Animals { get; } =
    [
        new("Белка", "A-104", "Собака", "Лабрадор", "Самка", "Серый", "Средний", "3 года", "24 кг", "Вакцинация от бешенства", "В приюте", "Не прикреплено", "12.09.2026"),
        new("Рекс", "A-107", "Собака", "Овчарка", "Самец", "Черно-рыжий", "Крупный", "5 лет", "31 кг", "Вакцинация выполнена", "В приюте", "Не прикреплено", "18.09.2026"),
        new("Барсик", "A-112", "Кошка", "Британская", "Самец", "Серый", "Средний", "2 года", "5 кг", "Наблюдение после лечения", "На лечении", "Не прикреплено", "21.09.2026"),
        new("Мурка", "A-118", "Кошка", "Дворовая", "Самка", "Черно-белый", "Маленький", "1 год", "4 кг", "Прививки отсутствуют", "Ожидает семью", "Не прикреплено", "23.09.2026"),
        new("Лада", "A-121", "Собака", "Метис", "Самка", "Белый", "Средний", "4 года", "18 кг", "Вакцинация выполнена", "В приюте", "Не прикреплено", "01.10.2026"),
        new("Тузик", "A-125", "Собака", "Метис", "Самец", "Коричневый", "Крупный", "7 лет", "22 кг", "Вакцинация выполнена", "Ожидает семью", "Не прикреплено", "04.10.2026")
    ];

    public static int AdoptedCount { get; set; } = 17;
}

public static class RequestStore
{
    public static List<RequestItem> Requests { get; } =
    [
        new()
        {
            Id = 1,
            ClientName = "Иванова Анна",
            Phone = "+7 900 111-11-11",
            TypeValue = RequestType.Adoption,
            AnimalNumber = "A-118",
            AnimalName = "Мурка",
            Date = new DateTime(2026, 10, 2)
        },
        new()
        {
            Id = 2,
            ClientName = "Петров Иван",
            Phone = "+7 900 222-22-22",
            TypeValue = RequestType.Surrender,
            AnimalDescription = "Собака, метис, самец, около 5 лет",
            Date = new DateTime(2026, 10, 1)
        }
    ];
}

public static class VeterinaryStore
{
    public static List<VeterinaryItem> Records { get; } =
    [
        new("Белка", "A-104", "Вакцинация", DateTime.Today, "Без дополнительных сведений"),
        new("Барсик", "A-112", "Осмотр", new DateTime(2026, 10, 1), "Без дополнительных сведений"),
        new("Рекс", "A-107", "Обработка", new DateTime(2026, 9, 30), "Без дополнительных сведений")
    ];
}
