// Views/UsersPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class UsersPage : ContentPage
{
    public UsersPage(UsersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Always reload data when the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UsersViewModel vm)
        {
            await vm.LoadUsersCommand.ExecuteAsync(null); // Execute the load command
            vm.SelectedUser = null; // Clear selection after loading/reloading
        }
    }
}