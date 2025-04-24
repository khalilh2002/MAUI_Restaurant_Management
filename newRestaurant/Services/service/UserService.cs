using Microsoft.EntityFrameworkCore;
using Microsoft.Maui;
using newRestaurant.Data;
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace newRestaurant.Services
{
    public class UserService : IUserService
    {
        private readonly RestaurantDbContext _context;

        public UserService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserAsync(int id) => await _context.Users.FindAsync(id);

        public async Task<User> GetUserByUsernameAsync(string username) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<bool> AddUserAsync(User user, string plainPassword) // Accept plain password
        {
            if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6) // Basic validation
            {
                Console.WriteLine("Error: Password is too short.");
                return false;
            }
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                Console.WriteLine($"Error: Username '{user.Username}' already exists.");
                return false;
            }
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                Console.WriteLine($"Error: Email '{user.Email}' already exists.");
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);


            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> VerifyPasswordAsync(string username, string providedPassword)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                return false; // User not found or no password stored
            }

            // --- Password Verification ---
            // ** REPLACE THIS with BCrypt in a real app! **
            // Simple placeholder check:
            // return user.PasswordHash == "HASHED_" + providedPassword;
            // ** Real Implementation using BCrypt.Net-Next **
            return BCrypt.Net.BCrypt.Verify(providedPassword, user.PasswordHash);
        }

        public Task<bool> AddUserAsync(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateUserAsync(User user) // user contains the ID and updated data
        {
            // 1. Check for conflicts (new username/email already used by OTHERS)
            if (await _context.Users.AnyAsync(u => u.Id != user.Id && u.Username == user.Username))
            {
                Console.WriteLine($"Error: Username '{user.Username}' is already taken by another user.");
                // Optionally throw exception or return specific result for ViewModel to handle
                return false;
            }
            if (await _context.Users.AnyAsync(u => u.Id != user.Id && u.Email == user.Email))
            {
                Console.WriteLine($"Error: Email '{user.Email}' is already used by another user.");
                return false;
            }

            // 2. Find the tracked entity
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                Console.WriteLine($"Error: User with ID {user.Id} not found for update.");
                return false;
            }

            // 3. Update allowed properties (Username, Email, Role)
            // DO NOT update PasswordHash here - password changes need a separate mechanism
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;

            // 4. Mark entity as modified (EF Core often detects changes, but explicit is fine)
            _context.Entry(existingUser).State = EntityState.Modified;

            // 5. Save changes
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user {user.Id}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            // Exclude sensitive data like PasswordHash from the list view if possible
            // For simplicity here, we return the full object.
            // Consider creating a UserViewModel/DTO if showing this to non-admins.
            return await _context.Users
                                 .OrderBy(u => u.Username) // Example ordering
                                 .ToListAsync();
        }

       
    }
}