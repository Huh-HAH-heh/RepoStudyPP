namespace StudyMauiApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnOpenShelterClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ShelterMainPage");
    }
}