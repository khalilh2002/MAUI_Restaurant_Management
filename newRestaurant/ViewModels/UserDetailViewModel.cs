using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using newRestaurant.Models;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq; // Needed for Cast/ToList
using System.Threading.Tasks;

namespace newRestaurant.ViewModels
{
    [QueryProperty(nameof(UserId), "UserId")]
    public partial class UserDetailViewModel : BaseViewModel
    {
        private int _userId;
        private bool _isInitialLoad = true;
        private string _originalUsername; // To check if username changed
        private string _originalEmail;    // To check if email changed

        [ObservableProperty] private string _editUsername;
        [ObservableProperty] private string _editEmail;
        [ObservableProperty] private UserRole _selectedRole;
        [ObservableProperty] private ObservableCollection<UserRole> _allRoles = new();
        [ObservableProperty] private bool _isCurrentUser;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(IsEditable))] private bool _canEdit; // Backing for IsEditable
        [ObservableProperty] private bool _isExistingUser; // Not strictly needed if UserId > 0 implies existing


        public bool IsEditable => CanEdit && !IsBusy; // Derived property for enabling controls

        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        // Public property for QueryProperty binding
        public int UserId
        {
            get => _userId;
            set
            {
                SetProperty(ref _userId, value);
                IsExistingUser = value > 0; // Set flag based on ID
                                            // Reset flag so InitializeAsync runs again if ID changes after appearing
                _isInitialLoad = true;
            }
        }

        public UserDetailViewModel(IUserService userService, INavigationService navigationService, IAuthService authService)
        {
            _userService = userService;
            _navigationService = navigationService;
            _authService = authService;
            Title = "User Details";

            // Populate roles once
            LoadRoles();
        }

        private void LoadRoles()
        {
            AllRoles.Clear();
            var roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
            foreach (var role in roles)
            {
                AllRoles.Add(role);
            }
        }

        // Called from Page's OnAppearing
        public async Task InitializeAsync()
        {
            if (!_isInitialLoad || IsBusy) return;

            IsBusy = true;
            CanEdit = false; // Disable editing while loading
            try
            {
                // Check if the user to be edited exists and is not the current user
                if (_userId > 0)
                {
                    var user = await _userService.GetUserAsync(_userId);
                    if (user != null)
                    {
                        // Check if this is the currently logged-in user
                        IsCurrentUser = (_authService.CurrentUser?.Id == user.Id);
                        CanEdit = !IsCurrentUser; // Enable editing only if NOT the current user

                        Title = IsCurrentUser ? $"View User: {user.Username}" : $"Edit User: {user.Username}";

                        EditUsername = user.Username;
                        EditEmail = user.Email;
                        SelectedRole = user.Role;

                        // Store originals for validation later
                        _originalUsername = user.Username;
                        _originalEmail = user.Email;

                        IsExistingUser = true;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "User not found.", "OK");
                        await GoBackAsync();
                        return; // Exit if not found
                    }
                }
                else // Should not happen if navigating from a list, but handle defensively
                {
                    Title = "Invalid User";
                    await Shell.Current.DisplayAlert("Error", "No user ID provided.", "OK");
                    await GoBackAsync();
                    return;
                }
                _isInitialLoad = false; // Mark load as complete
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing UserDetailViewModel: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load user details.", "OK");
            }
            finally
            {
                IsBusy = false;
                // Explicitly raise property changed for IsEditable after IsBusy changes
                OnPropertyChanged(nameof(IsEditable));
            }
        }


        [RelayCommand(CanExecute = nameof(IsEditable))] // Only allow save if editable and not busy
        private async Task SaveUserAsync()
        {
            if (!CanEdit) // Double check permission
            {
                await Shell.Current.DisplayAlert("Error", "Cannot edit this user.", "OK");
                return;
            }

            // --- Basic Validation ---
            if (string.IsNullOrWhiteSpace(EditUsername) || string.IsNullOrWhiteSpace(EditEmail))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Username and Email cannot be empty.", "OK");
                return;
            }
            // Add email format validation if desired
            bool isEmailValid = true; // Replace with actual email validation logic
            try { var addr = new System.Net.Mail.MailAddress(EditEmail); isEmailValid = (addr.Address == EditEmail); }
            catch { isEmailValid = false; }
            if (!isEmailValid)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please enter a valid email address.", "OK");
                return;
            }
            // --- End Validation ---


            IsBusy = true;
            bool success = false;
            try
            {
                // --- Check for Username/Email Conflicts ---
                // Only check if the username/email actually changed to avoid unnecessary lookups
                bool usernameChanged = !string.Equals(EditUsername, _originalUsername, StringComparison.OrdinalIgnoreCase);
                bool emailChanged = !string.Equals(EditEmail, _originalEmail, StringComparison.OrdinalIgnoreCase);

                if (usernameChanged)
                {
                    var existingUser = await _userService.GetUserByUsernameAsync(EditUsername);
                    if (existingUser != null && existingUser.Id != _userId)
                    {
                        await Shell.Current.DisplayAlert("Validation Error", $"Username '{EditUsername}' is already taken.", "OK");
                        return; // Stop saving
                    }
                }
                if (emailChanged)
                {
                    // Need a GetUserByEmail in service or modify check in UpdateUserAsync
                    // For now, rely on the check inside UpdateUserAsync
                }
                // --- End Conflict Check ---


                // Create a NEW object for updating
                User userToSave = new User
                {
                    Id = _userId, // Pass the ID
                    Username = EditUsername.Trim(),
                    Email = EditEmail.Trim().ToLower(),
                    Role = SelectedRole
                    // DO NOT PASS PasswordHash
                };

                success = await _userService.UpdateUserAsync(userToSave);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "User updated successfully.", "OK");
                    await GoBackAsync(); // Go back to the user list
                }
                else
                {
                    // Service layer might provide more specific errors (e.g., duplicate email)
                    await Shell.Current.DisplayAlert("Error", "Failed to update user. Check logs or ensure Username/Email are unique.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving user: {ex}");
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                // Re-evaluate CanExecute for the save button
                SaveUserCommand.NotifyCanExecuteChanged();
            }
        }


        [RelayCommand]
        private async Task GoBackAsync()
        {
            if (IsBusy) return; // Prevent double navigation
            await _navigationService.GoBackAsync();
        }

        // Override CanExecute property for RelayCommand
        // This property is derived from other observable properties
        // partial void OnCanEditChanged(bool value) => SaveUserCommand.NotifyCanExecuteChanged();
        // partial void OnIsBusyChanged(bool value) => SaveUserCommand.NotifyCanExecuteChanged();
        // -- NOTE: NotifyPropertyChangedFor(nameof(IsEditable)) on CanEdit handles this linkage more cleanly.
    }
}