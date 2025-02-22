namespace SpendBuddy.Models
{
    public class Expense
    {
        public int? ExpenseID {get; set; }
        public int? UserID {get; set; }

        public decimal Amount {get; set; } = 0;

        public string Name {get; set; } = "";
        
        public string Category {get; set; }

        public DateOnly Timestamp { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public string? Notes { get; set; }
    }

    // Wrapper class to handle posting expenses and their tags
    public class ExpenseWithTags : Expense
    {
        public HashSet<string> Tags { get; set; } = new();

        public ExpenseWithTags(Expense expense, HashSet<string> tags)
        {
            this.UserID = expense.UserID;
            this.Amount = expense.Amount;
            this.Name = expense.Name;
            this.Category = expense.Category;
            this.Timestamp = expense.Timestamp;
            this.Notes = expense.Notes;

            // Add the tags
            this.Tags = tags;
        }
    }

    public class ExpenseTagPair
    {
        public int ExpenseID { get; set; }
        public string Tag { get; set; } = string.Empty;
    }

    // Wrapper class to handle GetExpenses API response
    public class GetExpensesResponse
    {
        public string Message {get; set; }
        public List<Expense> Expenses { get; set; } = new();
        public List<ExpenseTagPair> ExpenseTagPairs { get; set; } = new();
    }

    public class GetCategoriesResponse
    {
        public string Message {get; set; }
        public HashSet<string> Categories { get; set; }
    }

    public class GetTagsResponse
    {
        public string Message {get; set; }
        public HashSet<string> Tags { get; set; }
    }

    public class UserTags
    {
        public int? UserID {get; set; }
        public HashSet<string> Tags {get; set; }
    }
}