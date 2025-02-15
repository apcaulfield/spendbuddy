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
        public int? UserID {get; private set; }

        public string ErrorMessage {get; set; } = "";

        public AccountService(HttpClient httpClient, ISessionStorageService sessionStorage)
        {
            _httpClient = httpClient;
            _sessionStorage = sessionStorage;
        }

        public async Task<bool> InitializeUserAsync(){
            int? storedUserID = await _sessionStorage.GetItemAsync<int?>("UserID");

            if (storedUserID.HasValue && storedUserID > 0)
            {
                // User ID has already been stored
                UserID = storedUserID.Value;
                return true;
            }

            return false;
        }

        // Attempt to add a new user to the database
        public async Task<bool> CreateUser(User newUser)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("CreateNewAccount", newUser);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<IDResponse>();
                    if (responseContent != null)
                    {
                        UserID = responseContent.ID;
                        await _sessionStorage.SetItemAsync("UserID", UserID);
                        // Console.WriteLine($"User logged in with ID: {UserID}");
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
                var response = await _httpClient.PostAsJsonAsync("login", loginCredentials);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<IDResponse>();
                    if (responseContent != null){
                        UserID = responseContent.ID;
                        await _sessionStorage.SetItemAsync("UserID", UserID);
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
