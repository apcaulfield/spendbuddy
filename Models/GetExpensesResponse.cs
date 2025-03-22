namespace SpendBuddy.Models
{
    public class GetExpensesResponse
    {
        public string Message { get; set; } = "";
        public List<Expense> Expenses { get; set; } = new();
        public List<ExpenseTagPair> ExpenseTagPairs { get; set; } = new();
    }
}
