namespace ByteMe.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalFoodItems { get; set; }
    public int ActiveOrders { get; set; }
    public int RegisteredUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalFeedback { get; set; }
    public int NewsletterSubscribers { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
    public List<TopFoodItemViewModel> TopFoodItems { get; set; } = new();
}

public class TopFoodItemViewModel
{
    public int FoodItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Price { get; set; }
}

public class CustomerListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

public class PaymentSummaryViewModel
{
    public decimal TotalRevenue { get; set; }
    public int CashOrders { get; set; }
    public int CardOrders { get; set; }
    public int EasyPaisaOrders { get; set; }
    public int NewsletterSubscribers { get; set; }
    public List<Order> Orders { get; set; } = new();
}
