using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpendBuddy.Models;
using System;

namespace SpendBuddy.Services
{
    public class AccountService
    {
        public ExpenseService ExpenseService { get; private set; }

        private readonly HttpClient _httpClient;
        public int UserID {get; private set; }

        public string ErrorMessage {get; set; } = "";

        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            ExpenseService = new ExpenseService(httpClient);
        }

        // Attempt to add a new user to the database
        public async Task<bool> CreateUser(User newUser)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("CreateNewAccount", newUser);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (responseContent != null)
                    {
                        UserID = responseContent.ID;
                        Console.WriteLine($"User logged in with ID: {UserID}");
                        return true;
                    }
                    else
                    {
                        ErrorMessage = "Empty response recieved when validate new user account.";
                    }
                }
                else
                {
                    ErrorMessage = "Validating user creation failed.";
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
                    var responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (responseContent != null){
                        UserID = responseContent.ID;
                        return true;
                    }
                    else{
                        ErrorMessage = "No valid user ID was sent back from the server.";
                    }
                }
                else
                {

                    ErrorMessage = $"Login failed: {response.ReasonPhrase}";
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
