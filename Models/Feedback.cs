namespace ByteMe.Models;

public class Feedback
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string FoodItemName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.Now;
    public bool IsApproved { get; set; } = false;
}
