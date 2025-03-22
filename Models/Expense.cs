namespace SpendBuddy.Models
{
    public class Expense
    {
        public int? ExpenseID { get; set; }
        public int? UserID { get; set; }

        public decimal Amount { get; set; } = 0;

        public string Name { get; set; } = "";
        
        public string Category { get; set; } = "";

        public DateOnly Timestamp { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public string? Notes { get; set; }
    }
}