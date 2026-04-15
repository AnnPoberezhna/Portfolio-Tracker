namespace PortfolioTracker.Models
{
    public class AssetViewModel
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public decimal Quantity { get; set; }
        public decimal CurrentValue { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PreviousPrice { get; set; }
        public decimal CurrentPrice { get; set; }

        public string PriceChangeClass
        {
            get
            {
                if (PreviousPrice <= 0)
                    return string.Empty;

                if (CurrentPrice > PreviousPrice)
                    return "price-up";
                else if (CurrentPrice < PreviousPrice)
                    return "price-down";
                else
                    return string.Empty;
            }
        }
    }
}
