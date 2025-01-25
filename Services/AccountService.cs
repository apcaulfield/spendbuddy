using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpendBuddy.Models;

namespace SpendBuddy.Services
{
    public class AccountService
    {
        private readonly HttpClient _httpClient;

        public string errorMessage = "";
        public bool isSuccessMessage = false;

        public User userCredentials = new User();

        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Make a login attempt with the provided credentials
        public async Task RequestLogin(User loginCredentials)
        {
            var response = _httpClient.PostAsJsonAsync("login", loginCredentials);
        }

        // Attempt to add a new user to the database
        public async Task CreateUser(User newUser)
        {
            var response = _httpClient.PostAsJsonAsync("addUser", newUser);
        }
    }
}
