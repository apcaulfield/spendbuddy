namespace SpendBuddy.Models
{
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
}