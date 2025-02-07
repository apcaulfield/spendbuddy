using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text.Json;
using SpendBuddy.Models;

namespace SpendBuddy.Services
{
    public class ExpenseService
    {
        // Contains both the expenses and a set of their tags
        public GetExpensesResponse ExpensesAndTags {get; private set; } = new();

        /* readonly: prevents reassignment of _expenses.
        IReadOnlyList: hides modification methods from other classes. */
        private List<Expense> _expenses = new();
        public IReadOnlyList<Expense> Expenses => _expenses;

        // Retrieve the entire category table
        private Dictionary<int, string> _categories = new Dictionary<int, string>();
        public ReadOnlyDictionary<int, string> Categories { get; }

        // Retrieve the entire tags table (for autofill)
        public HashSet<string> Tags {get; set; } = new(); 

        // List of expense-tag pairs for the expenses that are currently in view
        private HashSet<Tuple<int, int>> _expenseTagPairs = new HashSet<Tuple<int, int>>();

        // Used to keep track of where dates should be placed in the journal.
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
            ExpensesAndTags = await _httpClient.GetFromJsonAsync<GetExpensesResponse>(url) ?? new GetExpensesResponse();
            _expenses = ExpensesAndTags.Expenses;
            _expenseTagPairs = ExpensesAndTags.ExpenseTagPairs;
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
            
            // Perform binary search if adding an expense from a previous day
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

            mostRecentTimestamp = null;

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
