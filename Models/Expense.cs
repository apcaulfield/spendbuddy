namespace SpendBuddy.Models
{
    public class Expense
    {
        public int? Expense_id {get; set; }
        public int? User_id {get; set; }

        public float Amount {get; set; } = 0;

        public string Name {get; set; } = "";
        
        public string Category {get; set; } = "";

        public DateOnly Timestamp { get; set; }

        public string? Description { get; set; }

        public string? Notes { get; set; }
    }
}