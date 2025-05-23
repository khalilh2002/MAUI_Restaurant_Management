// Views/UserDetailPage.xaml.cs
using newRestaurant.ViewModels;

namespace newRestaurant.Views;

public partial class UserDetailPage : ContentPage
{
    public UserDetailPage(UserDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Call the ViewModel's initialization logic when the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UserDetailViewModel vm)
        {
            await vm.InitializeAsync(); // Call the async initialization
        }
    }
}