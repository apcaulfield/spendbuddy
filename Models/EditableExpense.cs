using SpendBuddy.Models;

namespace SpendBuddy.Models
{
    public class EditableExpense
    {
        public Expense Expense {get; set; } = new();

        // Tags that existed at the time the edit modal was opened
        public HashSet<string> InitialTags {get; set; } = new();

        /* All tags including inital and new ones added.
        New tags for the expense are determined by subtracting this
        set from the initial set when the edit is complete. */
        public HashSet<string> AllTags {get; set; } = new();
        
        // Tags that didn't exist in the database for user previously\
        public HashSet<string> BrandNewTags {get; set; } = new();

        public string NewTag {get; set; } = "";

    }
}