namespace SpendBuddy.Models
{
    public class User
    {
        public int? ID {get; set; }
        public string Username {get; set; } = "";
        public string Password {get; set; } = "";
    }

    // Class for parsing HTTP responses of this format
    public class IDResponse
    {
        public required int ID {get; set; }
        public required string Message {get; set; }
    }
}