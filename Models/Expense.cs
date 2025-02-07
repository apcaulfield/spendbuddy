using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SpendBuddy.Models
{
    public class Expense : IValidatableObject
    {
        public int? ExpenseID {get; set; }
        public int? UserID {get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount {get; set; } = 0;

        [Required(ErrorMessage = "Name is required.")]
        public string Name {get; set; } = "";
        
        [Required(ErrorMessage = "Category is required.")]
        public int CategoryID {get; set; }

        public DateOnly Timestamp { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public HashSet<string> Tags { get; set; } = new();

        public string? Notes { get; set; }

        // Handles all other validations (Timestamp)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Timestamp > DateOnly.FromDateTime(DateTime.Today))
            {
                yield return new ValidationResult(
                    "Date cannot be in the future.",
                    new[] { nameof(Timestamp) }
                );
            }
        }
    }

    // Wrapper class to handle GetExpenses API response
    public class GetExpensesResponse
    {
        public List<Expense> Expenses { get; set; }
        public HashSet<Tuple<int, int>> ExpenseTagPairs { get; set; }
    }
}