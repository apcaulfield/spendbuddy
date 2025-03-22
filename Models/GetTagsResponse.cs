namespace SpendBuddy.Models
{
    public class GetTagsResponse
    {
        public string Message { get; set; } = "";
        public required HashSet<string> Tags { get; set; }
    }
}
