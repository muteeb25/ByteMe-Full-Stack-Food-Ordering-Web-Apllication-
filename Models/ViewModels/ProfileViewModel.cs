using ByteMe.Models;

namespace ByteMe.Models.ViewModels;

public class ProfileViewModel
{
    public User User { get; set; } = null!;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public string FavoriteCategory { get; set; } = "N/A";
    public List<Order> RecentOrders { get; set; } = new();
}
