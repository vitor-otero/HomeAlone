namespace MyApi
{
    public class Payment
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string DueDate { get; set; }
        public bool IsPaid { get; set; }
    }
}
