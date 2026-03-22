using System;
using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Models
{
    public class Asset{

        // Primary key for the database
        [Key]
        public int Id {get; set; }

        // Asset name, for example "Apple"
        [Required(ErrorMessage = "Please provide the asset name.")]
        [MaxLength(50)]
        public string Name { get; set; } 

        // Ticker symbol, for example "AAPL" or "BTC"(can be used for fetching data from the API)
        [Required(ErrorMessage = "Please provide the symbol.")]
        [MaxLength(10)]
        public string Symbol { get; set; } 

        // Quantity of shares/crypto owned
        [Required]
        [Range(0.0001, 1000000, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; } 

        // Price paid per single unit
        [Required]
        [Range(0.01, 100000000, ErrorMessage = "Purchase price must be greater than zero.")]
        public decimal PurchasePrice { get; set; } 

        // Purchase date (defaults to the moment the entry is added)
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

    }
}