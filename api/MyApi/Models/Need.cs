namespace MyApi
{
    public class Need
    {
        public int Id { get; set; } // Add this property
        public string Description { get; set; }
        public bool IsMet { get; set; } // Add this property
        public int PercentageMet { get; set; } // Add this property
    }
}
