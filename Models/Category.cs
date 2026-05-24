namespace ByteMe.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
}
