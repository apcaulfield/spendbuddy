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

        // Used to keep track of whether or not a new expense should be added to the front of the list or not.
        public DateOnly? mostRecentTimestamp;

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

            var responseObject = JsonSerializer.Deserialize<IDResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (responseObject?.ID == null || responseObject?.ID == 0){
                throw new Exception("No ID was returned after adding expense.");
            }

            expense.ExpenseID = responseObject.ID;
            if (expense.Timestamp != DateOnly.FromDateTime(DateTime.Today))
            {
                int insertIndex = _expenses.BinarySearch(expense, new ExpenseTimestampComparer());
                if (insertIndex < 0)
                {
                    // If not found, BinarySearch returns (~index), where index is the insertion point.
                    insertIndex = ~insertIndex;
                }
                else
                {
                    // If found, move backwards to the lowest index with the same timestamp.
                    while (insertIndex > 0 && _expenses[insertIndex - 1].Timestamp == expense.Timestamp)
                    {
                        insertIndex--;
                    }
                }
                _expenses.Insert(insertIndex, expense);
            }
            else{
                _expenses.Insert(0, expense);
            }

            // Refresh pages
            OnExpensesUpdated?.Invoke();

            return responseObject.ID;
        }
    }

    class ExpenseTimestampComparer : IComparer<Expense>
    {
        public int Compare(Expense x, Expense y)
        {
            return y.Timestamp.CompareTo(x.Timestamp);
        }
    }
}
