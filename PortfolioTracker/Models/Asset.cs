using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Models
{
    public class Asset : IValidatableObject
    {
        // Primary key for the database
        [Key]
        public int Id { get; set; }

        // Ticker symbol, for example "AAPL" or "BTC"(can be used for fetching data from the API)
        [Required(ErrorMessage = "Please select a cryptocurrency.")]
        [MaxLength(10)]
        public string Symbol { get; set; } 

        // Quantity of shares/crypto owned
        [Required]
        [Range(0.0001, 1000000, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; }

        // Calculated current value (price * quantity)
        [Required]
        public decimal CurrentValue { get; set; } 

        // Purchase date (defaults to the moment the entry is added)
        [Required(ErrorMessage = "Please provide the purchase date.")]
        [DataType(DataType.DateTime)]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var minDate = new DateTime(1900, 1, 1);
            var maxDate = DateTime.Now;

            if (PurchaseDate < minDate || PurchaseDate > maxDate)
            {
                yield return new ValidationResult(
                    "Purchase date must be between 1900-01-01 and today.",
                    new[] { nameof(PurchaseDate) });
            }
        }

    }
}