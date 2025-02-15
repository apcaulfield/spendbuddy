using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.SessionStorage;
using SpendBuddy.Models;
using System;

namespace SpendBuddy.Services
{
    public class AccountService
    {
        private readonly ISessionStorageService _sessionStorage;
        private readonly HttpClient _httpClient;

        public string ErrorMessage {get; set; } = "";

        public AccountService(HttpClient httpClient, ISessionStorageService sessionStorage)
        {
            _httpClient = httpClient;
            _sessionStorage = sessionStorage;
        }

        // Attempt to add a new user to the database
        public async Task<bool> CreateUser(User newUser)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("User", newUser);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<IDResponse>();
                    if (responseContent?.ID != null)
                    {
                        await _sessionStorage.SetItemAsync("UserID", responseContent.ID);
                        // Console.WriteLine($"User logged in with ID: {userID}");
                        return true;
                    }
                    else
                    {
                        ErrorMessage = "Empty response recieved when validate new user account.";
                    }
                }
                else
                {
                    ErrorMessage = "Account with username already exists.";
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while creating the account: {ex.Message}";
                return false;
            }
        }

        // Make a login attempt with the provided credentials
        public async Task<bool> RequestLogin(User loginCredentials)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Login", loginCredentials);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<IDResponse>();
                    if (responseContent?.ID != null){
                        await _sessionStorage.SetItemAsync("UserID", responseContent.ID);
                        return true;
                    }
                    else{
                        ErrorMessage = "No valid user ID was sent back from the server.";
                    }
                }
                else
                {

                    ErrorMessage = "Invalid username or password.";
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during login: {ex.Message}";
                return false;
            }
        }
    }
}
