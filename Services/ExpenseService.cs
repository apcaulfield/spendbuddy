using System.Net.Http.Json;
using System.Text.Json;
using SpendBuddy.Models;

namespace SpendBuddy.Services
{
    public class ExpenseService
    {
        /* readonly: prevents reassignment of _expenses.
        IReadOnlyList: hides modification methods from other classes. */
        private List<Expense> _expenses = new();
        public IReadOnlyList<Expense> Expenses => _expenses;

        // List of expense-tag pairs for the expenses that are currently in view
        public List<ExpenseTagPair> ExpenseTagPairs { get; set; } = new();

        public HashSet<string> Categories {get; private set; } = new();
        public HashSet<string> Tags {get; private set; } = new();

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
        public async Task FetchPageOfExpensesAsync(int userID, int pageIndex = 0)
        {
            if (userID == 0){
                throw new InvalidOperationException("Cannot fetch expenses without a user ID.");
            }
            
            string url = "Expense?" +
            $"userID={userID}" +
            $"&page={pageIndex}";

            GetExpensesResponse response = await _httpClient.GetFromJsonAsync<GetExpensesResponse>(url) ?? new GetExpensesResponse();
            _expenses = response.Expenses;
            ExpenseTagPairs = response.ExpenseTagPairs;
        }

        public async Task FetchAllCategoriesAsync(int userID)
        {
            if (userID == 0){
                throw new InvalidOperationException("Cannot fetch categories without user ID.");
            }
            string url = "Category?" + $"userID={userID}";
            GetCategoriesResponse? categoryResponse = await _httpClient.GetFromJsonAsync<GetCategoriesResponse>(url);
            Categories = categoryResponse?.Categories ?? new HashSet<string>();
        }

        public async Task FetchAllTagsAsync(int userID)
        {
            if (userID == 0){
                throw new InvalidOperationException("Cannot fetch tags without a user ID.");
            }
            string url = "Tag?" + $"userID={userID}";
            GetTagsResponse? tagResponse = await _httpClient.GetFromJsonAsync<GetTagsResponse>(url);
            Tags = tagResponse?.Tags ?? new HashSet<string>();
        }

        // Add expense via API
        public async Task<int> AddExpenseAsync(Expense expense, HashSet<string> tags)
        {
            ExpenseWithTags expenseToSubmit = new ExpenseWithTags(expense, tags);
            var response = await _httpClient.PostAsJsonAsync("Expense", expenseToSubmit);

            if (!response.IsSuccessStatusCode)
            {
                // Failed to add expense to database
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
            expense.ExpenseID = responseObject!.ID;

            foreach (string tag in tags){
                ExpenseTagPairs.Add(new ExpenseTagPair {ExpenseID = responseObject.ID, Tag = tag });
            }
            
            InsertExpense(expense);

            // Refresh pages
            OnExpensesUpdated?.Invoke();

            return responseObject.ID;
        }

        public async Task<bool> AddCategoryAsync(int userID, string newCategory)
        {
            if (userID == 0){
                throw new InvalidOperationException("Cannot add a category without a user ID.");
            }

            if (Categories.Contains(newCategory)){
                // Already exists - don't add
                return false;
            }
            var data = new
            {
                userID = userID,
                category = newCategory
            };

            var response = await _httpClient.PostAsJsonAsync("Category", data);

            if (!response.IsSuccessStatusCode)
            {
                // Error adding category
                return false;
            }
            
            Categories.Add(newCategory);
            return true;
        }

        
        public async Task<bool> AddUserTagsAsync(int userID, HashSet<string> newTags)
        {
            if (userID == 0){
                throw new InvalidOperationException("Cannot add tags without a user ID.");
            }

            var data = new {
                userID=userID,
                tags=newTags
            };

            var response = await _httpClient.PostAsJsonAsync("Tag", data);
            if (!response.IsSuccessStatusCode)
            {
                // Error adding tags
                return false;
            }
            
            Tags.UnionWith(newTags);
            return true;
        }

        public async Task<bool> UpdateExpenseAsync(Expense expense, HashSet<string> tags)
        {
            if (expense.ExpenseID == null)
            {
                throw new Exception("ExpenseID cannot be null when updating expense.");
            }

            ExpenseWithTags expenseToUpdate = new ExpenseWithTags(expense, tags);
            var response = await _httpClient.PatchAsJsonAsync($"Expense/{expense.ExpenseID}", expenseToUpdate);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to update expense: {response.StatusCode}");
            }

            _expenses.RemoveAll(e => e.ExpenseID == expense.ExpenseID);
            InsertExpense(expense);

            ExpenseTagPairs.RemoveAll(pair => pair.ExpenseID == expense.ExpenseID);
            foreach (var tag in tags)
            {
                ExpenseTagPairs.Add(new ExpenseTagPair { ExpenseID = expense.ExpenseID.Value, Tag = tag });
            }

            OnExpensesUpdated?.Invoke();
            return true;
        }

        public void InsertExpense(Expense expense)
        {
            // Reinsert the updated expense using similar sorting logic as in AddExpenseAsync
            if (expense.Timestamp != DateOnly.FromDateTime(DateTime.Today))
            {
                int insertIndex = _expenses.BinarySearch(expense, new ExpenseTimestampComparer());
                if (insertIndex < 0)
                {
                    insertIndex = ~insertIndex;
                }
                else
                {
                    while (insertIndex > 0 && _expenses[insertIndex - 1].Timestamp == expense.Timestamp)
                    {
                        insertIndex--;
                    }
                }
                _expenses.Insert(insertIndex, expense);
            }
            else
            {
                _expenses.Insert(0, expense);
            }
            
            mostRecentTimestamp = null;
        }
    }

    class ExpenseTimestampComparer : IComparer<Expense>
    {
        public int Compare(Expense? x, Expense? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return y.Timestamp.CompareTo(x.Timestamp);
        }
    }
}
