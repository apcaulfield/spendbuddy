using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpendBuddy.Models;

namespace SpendBuddy.Services
{
    public class ExpenseService
    {
        /* readonly: prevents reassignment of _expenses.
        IReadOnlyList: hides modification methods from other classes. */
        private List<Expense> _expenses = new();
        public IReadOnlyList<Expense> Expenses => _expenses;

        private readonly HttpClient _httpClient;

        public ExpenseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Fetch expenses from API
        public async Task FetchAllExpensesAsync()
        {
            _expenses = await _httpClient.GetFromJsonAsync<List<Expense>>("expenses") ?? new List<Expense>();
        }

        // Add expense via API
        public async Task AddExpenseAsync(Expense expense)
        {
            var response = await _httpClient.PostAsJsonAsync("AddExpense", expense);
            if (response.IsSuccessStatusCode)
            {
                _expenses.Add(expense);
            }
        }
    }
}
