namespace MyApi.Models
{
    public class Pending
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
