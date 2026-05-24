// PATTERN: FACTORY
using ByteMe.Models;

namespace ByteMe.Services.Patterns;

public static class FoodItemFactory
{
    public static FoodItem Create(string category)
    {
        var item = new FoodItem
        {
            Name = string.Empty,
            Price = 0,
            IsAvailable = true
        };

        switch (category.ToLowerInvariant())
        {
            case "pizza":
                item.Description = "[Pizza] Fresh oven-baked ";
                item.ImageUrl = "/images/food/default-pizza.jpg";
                break;
            case "burgers":
                item.Description = "[Burger] Flame-grilled ";
                item.ImageUrl = "/images/food/default-burger.jpg";
                break;
            case "sandwiches":
                item.Description = "[Sandwich] Handcrafted ";
                item.ImageUrl = "/images/food/default-sandwich.jpg";
                break;
            case "pasta":
                item.Description = "[Pasta] Italian-style ";
                item.ImageUrl = "/images/food/default-pasta.jpg";
                break;
            default:
                item.Description = "[ByteMe] Delicious ";
                item.ImageUrl = "/images/food/default.jpg";
                break;
        }

        return item;
    }
}
