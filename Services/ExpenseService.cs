using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpendBuddy.Models;
using System.Text.Json;

namespace SpendBuddy.Services
{
    public class ExpenseService
    {
        /* readonly: prevents reassignment of _expenses.
        IReadOnlyList: hides modification methods from other classes. */
        private List<Expense> _expenses = new();
        public IReadOnlyList<Expense> Expenses => _expenses;

        private readonly HttpClient _httpClient;

        // Subscribable refresh for components
        public event Action? OnExpensesUpdated;

        public ExpenseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Fetch expenses from API
        public async Task FetchAllExpensesAsync(int userID, int pageIndex = 0)
        {
            string url = "GetExpenses?" +
            $"userID={userID}" +
            $"&page={pageIndex}";
            _expenses = await _httpClient.GetFromJsonAsync<List<Expense>>(url) ?? new List<Expense>();
        }

        // Add expense via API
        public async Task<int> AddExpenseAsync(Expense expense)
        {
            var response = await _httpClient.PostAsJsonAsync("AddExpense", expense);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to add expense: {response.StatusCode}");
            }

            // Adding expense was a success
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);

            var responseObject = JsonSerializer.Deserialize<IDResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (responseObject.ID == null || responseObject.ID == 0){
                throw new Exception("No ID was returned after adding expense.");
            }

            Console.WriteLine($"RESPONSEOBJECT.ID: {responseObject.ID}");

            expense.ExpenseID = responseObject.ID;
            //Console.WriteLine(JsonSerializer.Serialize(expense, new JsonSerializerOptions { WriteIndented = true }));
            _expenses.Insert(0, expense);

            // Refresh pages
            OnExpensesUpdated?.Invoke();

            return responseObject.ID;
        }
    }
}
