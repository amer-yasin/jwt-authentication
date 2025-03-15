namespace api.Models.Entities
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Completed { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}