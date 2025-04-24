// ViewModels/UsersViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;

// using newRestaurant.Services; // Not needed if using interfaces only
using newRestaurant.Services.Interfaces;
using newRestaurant.Views; // Needed for potential nameof navigation
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;


namespace newRestaurant.ViewModels
{
    public partial class UsersViewModel : BaseViewModel // Ensure BaseViewModel exists
    {
        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private User _selectedUser; // For potential detail navigation

        private readonly IUserService _userService;
        private readonly INavigationService _navigationService; // Keep for navigation

        public UsersViewModel(IUserService userService, INavigationService navigationService)
        {
            _userService = userService;
            _navigationService = navigationService;
            Title = "Manage Users"; // Set a title
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Users.Clear(); // Use the generated property name (_users is backing field)
                var usersList = await _userService.GetUsersAsync();
                foreach (var user in usersList)
                {
                    Users.Add(user); // Add to the public property
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading users: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Placeholder command for future detail view navigation
        [RelayCommand]
        private async Task GoToUserDetailAsync()
        {
            if (SelectedUser == null || IsBusy) return;

            // --- Navigate to the actual detail page ---
            await _navigationService.NavigateToAsync(nameof(UserDetailPage),
               new Dictionary<string, object> { { "UserId", SelectedUser.Id } });

            // await Shell.Current.DisplayAlert("Info", $"Selected User: {SelectedUser.Username} (Detail page not implemented)", "OK"); // Remove placeholder

            // Keep deselection if desired, or let OnAppearing handle it
            // SelectedUser = null;
        }

        // Placeholder command for future add user navigation
        [RelayCommand]
        private async Task AddUserAsync()
        {
            if (IsBusy) return;
            // Navigate to Register page
            await _navigationService.NavigateToAsync(nameof(RegisterPage));
            // await Shell.Current.DisplayAlert("Info", "Add User UI not implemented here. Use Register Page.", "OK"); // Remove placeholder
        }
    }
}