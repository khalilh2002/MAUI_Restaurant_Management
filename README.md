# MAUI Restaurant Management App (Windows) 🍽️

A .NET 8 MAUI application designed specifically for Windows to manage various aspects of a restaurant operation. Built following the MVVM pattern using the Community Toolkit and Entity Framework Core with a MySQL database.

## ✨ Features

*   **Menu Management:**
    *   Add, edit, and delete food/dish **Categories**.
    *   Add, edit, and delete **Dishes (Plats)** including name, description, price, and assigning them to categories.
*   **Ordering & Cart System:**
    *   Browse dishes and add them to a shopping **Cart**.
    *   View the cart contents.
    *   Increase/decrease item quantities in the cart.
    *   Remove items from the cart.
    *   View the calculated total price.
    *   **Simulate Payment/Checkout:** Mark the cart as ordered (no real payment processing).
*   **Table Management:**
    *   Add, edit, and delete restaurant **Tables** with number and capacity.
*   **Table Reservations:**
    *   View existing reservations.
    *   Create new reservations for specific users, tables, dates, and times.
    *   Edit existing reservation details.
    *   Delete reservations (with checks for active/upcoming ones).
    *   Basic overlap detection for new reservations on the same table.
*   **User Authentication & Management:**
    *   User **Registration** with username, email, and password (password hashing implemented - requires BCrypt.Net-Next).
    *   User **Login**.
    *   Basic **User List** view (Username, Email, Role) for administrators.
    *   **Edit User Details:** Administrators can edit the Username, Email, and Role of other users (cannot edit the currently logged-in user via this screen).

*(Note: Statistics & Reports feature is not implemented in this version.)*

## 💻 Technology Stack

*   **Framework:** .NET 8 MAUI (Targeting **Windows** only)
*   **Language:** C#
*   **Architecture:** Model-View-ViewModel (MVVM)
*   **MVVM Toolkit:** CommunityToolkit.Mvvm
*   **Database:** MySQL (or MariaDB)
*   **ORM:** Entity Framework Core 8+ (Using `Pomelo.EntityFrameworkCore.MySql` provider)
*   **UI:** .NET MAUI XAML
*   **Password Hashing:** BCrypt.Net-Next (Recommended - ensure installed and used in `UserService`)

## 🚀 Setup and Installation

### Prerequisites

1.  **.NET 9 SDK / NET 8 SDK :** Download and install from the official Microsoft website.
2.  **Visual Studio 2022:** Version 17.9 Preview or later recommended for .NET 9 support. Ensure the ".NET Multi-platform App UI development" workload is installed.
3.  **MySQL Server (xamp):** A running instance of MySQL or MariaDB (v5.7 or later recommended for Pomelo provider). Note the server IP address/hostname, port (usually 3306), username, and password.
4.  **Git:** For cloning the repository.

### Steps

1.  **Clone the Repository:**
    ```bash
    git clone https://github.com/khalilh2002/MAUI_Restaurant_Management.git
    ```

2.  **Database Setup:**
    *   **Create Database:** Using a MySQL administration tool (like MySQL Workbench, HeidiSQL, phpMyAdmin via XAMPP, or command line), create a new database. Let's assume you name it `amine` (as used in the example code).
        ```sql
        CREATE DATABASE amine;
        ```
    *   **Configure Connection String:** You **must** update the database connection details in **two** files:
        *   `Data/RestaurantDbContextFactory.cs`: Used by EF Core tools (`Add-Migration`, `Update-Database`).
        *   `MauiProgram.cs`: Used by the application at runtime.
        Open these files and replace the placeholder values for `server`, `port`, `database`, `user`, and `password` with your actual MySQL server details.
        ```csharp
        // Example section in both files:
        const string server = "YOUR_MYSQL_IP_OR_HOSTNAME"; // e.g., "localhost", "192.168.0.30"
        const string port = "3306";
        const string database = "amine"; // Your database name
        const string user = "YOUR_MYSQL_USERNAME"; // e.g., "root" or a dedicated user
        const string password = "YOUR_MYSQL_PASSWORD";
        string connectionString = $"Server={server};Port={port};Database={database};Uid={user};Pwd={password};";
        ```
    *  

3.  **Install NuGet Packages:**
    *   Open the solution (`.sln` file) in Visual Studio.
    *   Right-click the Solution in Solution Explorer -> "Restore NuGet Packages".
    *   Ensure `Pomelo.EntityFrameworkCore.MySql` and `CommunityToolkit.Mvvm` are installed. Install `BCrypt.Net-Next` if you haven't already for password hashing.

4.  **Build the Solution:**
    *   Build -> Clean Solution.
    *   Build -> Rebuild Solution.

## ▶️ Running the Application

1.  **Set Startup Project:** Ensure `newRestaurant` (or your project name) is set as the Startup Project in Visual Studio.
2.  **Select Target:** Choose the **Windows Machine** target framework from the debug dropdown (it should look like `net9.0-windows10.0.xxxxx`).
3.  **Run:** Press `F5` or click the "Start" button (with the Windows icon).
4.  **Database Migration:** On the very first run, Entity Framework Core migrations should automatically be applied (triggered by the code in `MauiProgram.cs`). This will create the necessary tables (`Categories`, `Plats`, `Users`, etc.) in your `amine` database.
    *   **Troubleshooting:** If the app fails to start with database errors, check the **Output** window in Visual Studio for specific connection or migration errors. Ensure your connection string and user permissions are correct. Verify the MySQL server is running and accessible.
5.  **Login/Register:** The application will start on the Login page. You can register a new user first.

---

Enjoy managing your restaurant with the NewRestaurant MAUI App!
