namespace SpendBuddy.Models
{
    public class GetCategoriesResponse
    {
        public string Message { get; set; } = "";
        public required HashSet<string> Categories { get; set; }
    }
}
