using ByteMe.Helpers;
using ByteMe.Models;
using Microsoft.EntityFrameworkCore;

namespace ByteMe.Data;

public static class SeedData
{
    public static void Initialize(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        Seed(context);
    }

    public static void Seed(AppDbContext context)
    {
        if (!context.Categories.Any())
        {
            SeedInitial(context);
            return;
        }

        EnsureMenuExpansion(context);
        EnsureUsersExist(context);
    }

    private static void SeedInitial(AppDbContext context)
    {
        var categories = CreateCategories();
        context.Categories.AddRange(categories);
        context.SaveChanges();

        context.FoodItems.AddRange(BuildAllFoodItems(categories));
        context.Users.AddRange(CreateDefaultUsers());
        context.SaveChanges();
    }

    private static void EnsureMenuExpansion(AppDbContext context)
    {
        if (context.Categories.Any(c => c.Name == "Drinks"))
            return;

        var pizza = context.Categories.First(c => c.Name == "Pizza");
        var burgers = context.Categories.First(c => c.Name == "Burgers");
        var sandwiches = context.Categories.First(c => c.Name == "Sandwiches");
        var pasta = context.Categories.First(c => c.Name == "Pasta");

        var grilled = context.FoodItems.FirstOrDefault(f => f.Name == "Grilled Chicken");
        if (grilled != null)
            grilled.ImageUrl = "https://images.unsplash.com/photo-1606755962773-d324e0a13086?w=400";

        var extraItems = new List<FoodItem>
        {
            new() { Name = "Veggie Supreme", Description = "Loaded garden vegetables on crispy crust.", Price = 780, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1511689660979-10d2b1afd49d?w=400" },
            new() { Name = "Chicken Tikka Pizza", Description = "Spiced tikka chicken with onions.", Price = 1100, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=400" },
            new() { Name = "Four Cheese", Description = "Mozzarella, cheddar, parmesan and gorgonzola.", Price = 1200, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1571407970349-bc81e7e96d47?w=400" },
            new() { Name = "Mushroom Delight", Description = "Fresh mushrooms and herbs.", Price = 900, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1506354666786-959d6d497f1a?w=400" },
            new() { Name = "Crispy Chicken", Description = "Crunchy chicken fillet burger.", Price = 520, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1585238342024-78d387f4a707?w=400" },
            new() { Name = "BBQ Bacon", Description = "Smoky BBQ with crispy bacon.", Price = 750, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=400" },
            new() { Name = "Veggie Burger", Description = "Plant-based patty with fresh greens.", Price = 380, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1520072959219-c595dc870360?w=400" },
            new() { Name = "Spicy Jalapeno", Description = "Fiery jalapenos and pepper jack.", Price = 600, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?w=400" },
            new() { Name = "Turkey Avocado", Description = "Sliced turkey with creamy avocado.", Price = 480, CategoryId = sandwiches.Id, ImageUrl = "https://images.unsplash.com/photo-1481070414801-51fd732d7184?w=400" },
            new() { Name = "Egg Mayo", Description = "Classic egg salad sandwich.", Price = 320, CategoryId = sandwiches.Id, ImageUrl = "https://images.unsplash.com/photo-1554433607-66b5ffe5d5e1?w=400" },
            new() { Name = "Philly Cheesesteak", Description = "Steak, peppers and melted cheese.", Price = 560, CategoryId = sandwiches.Id, ImageUrl = "https://images.unsplash.com/photo-1600555379765-f8e1e4b4c8b4?w=400" },
            new() { Name = "Mac and Cheese", Description = "Creamy macaroni in cheese sauce.", Price = 420, CategoryId = pasta.Id, ImageUrl = "https://images.unsplash.com/photo-1543352634-a1c51d9f1fa7?w=400" },
            new() { Name = "Bolognese", Description = "Rich meat sauce over spaghetti.", Price = 550, CategoryId = pasta.Id, ImageUrl = "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400" },
            new() { Name = "Lasagna", Description = "Layered pasta with beef and cheese.", Price = 680, CategoryId = pasta.Id, ImageUrl = "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=400" }
        };

        var drinks = new Category { Name = "Drinks", ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" };
        var deals = new Category { Name = "Deals", ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400" };
        context.Categories.AddRange(drinks, deals);
        context.SaveChanges();

        extraItems.AddRange(new[]
        {
            new FoodItem { Name = "Mango Shake", Description = "Fresh mango blended smooth.", Price = 220, CategoryId = drinks.Id, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" },
            new FoodItem { Name = "Oreo Milkshake", Description = "Creamy Oreo cookie shake.", Price = 280, CategoryId = drinks.Id, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" },
            new FoodItem { Name = "Fresh Lemonade", Description = "Zesty homemade lemonade.", Price = 180, CategoryId = drinks.Id, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" },
            new FoodItem { Name = "Cold Coffee", Description = "Iced coffee with milk.", Price = 250, CategoryId = drinks.Id, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" },
            new FoodItem { Name = "Student Deal", Description = "Burger + drink + fries combo.", Price = 650, CategoryId = deals.Id, ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400" },
            new FoodItem { Name = "Family Deal", Description = "2 pizzas + 4 drinks for the family.", Price = 2200, CategoryId = deals.Id, ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400" },
            new FoodItem { Name = "Lunch Special", Description = "Sandwich + drink lunch combo.", Price = 480, CategoryId = deals.Id, ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400" }
        });

        context.FoodItems.AddRange(extraItems);
        context.SaveChanges();
    }

    private static void EnsureUsersExist(AppDbContext context)
    {
        if (context.Users.Any()) return;
        context.Users.AddRange(CreateDefaultUsers());
        context.SaveChanges();
    }

    private static List<Category> CreateCategories() => new()
    {
        new() { Name = "Pizza", ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400" },
        new() { Name = "Burgers", ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400" },
        new() { Name = "Sandwiches", ImageUrl = "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=400" },
        new() { Name = "Pasta", ImageUrl = "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400" },
        new() { Name = "Drinks", ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" },
        new() { Name = "Deals", ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400" }
    };

    private static List<FoodItem> BuildAllFoodItems(List<Category> categories)
    {
        var pizza = categories[0];
        var burgers = categories[1];
        var sandwiches = categories[2];
        var pasta = categories[3];
        var drinks = categories[4];
        var deals = categories[5];

        return new List<FoodItem>
        {
            new() { Name = "Margherita", Description = "Classic tomato, mozzarella and basil pizza.", Price = 850, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400" },
            new() { Name = "BBQ Chicken", Description = "Smoky BBQ sauce with grilled chicken.", Price = 1050, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400" },
            new() { Name = "Pepperoni", Description = "Spicy pepperoni with melted cheese.", Price = 950, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=400" },
            new() { Name = "Veggie Supreme", Description = "Loaded garden vegetables on crispy crust.", Price = 780, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1511689660979-10d2b1afd49d?w=400" },
            new() { Name = "Chicken Tikka Pizza", Description = "Spiced tikka chicken with onions.", Price = 1100, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1565299585323-38d6b0865b47?w=400" },
            new() { Name = "Four Cheese", Description = "Mozzarella, cheddar, parmesan and gorgonzola.", Price = 1200, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1571407970349-bc81e7e96d47?w=400" },
            new() { Name = "Mushroom Delight", Description = "Fresh mushrooms and herbs.", Price = 900, CategoryId = pizza.Id, ImageUrl = "https://images.unsplash.com/photo-1506354666786-959d6d497f1a?w=400" },
            new() { Name = "Classic Smash", Description = "Juicy beef patty with fresh veggies.", Price = 450, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400" },
            new() { Name = "Zinger", Description = "Crispy spicy chicken fillet burger.", Price = 550, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1550547660-d9450f859349?w=400" },
            new() { Name = "Double Patty", Description = "Two beef patties, double the flavor.", Price = 650, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1586816001966-79b736744398?w=400" },
            new() { Name = "Crispy Chicken", Description = "Crunchy chicken fillet burger.", Price = 520, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1585238342024-78d387f4a707?w=400" },
            new() { Name = "BBQ Bacon", Description = "Smoky BBQ with crispy bacon.", Price = 750, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=400" },
            new() { Name = "Veggie Burger", Description = "Plant-based patty with fresh greens.", Price = 380, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1520072959219-c595dc870360?w=400" },
            new() { Name = "Spicy Jalapeno", Description = "Fiery jalapenos and pepper jack.", Price = 600, CategoryId = burgers.Id, ImageUrl = "https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?w=400" },
            new() { Name = "Club Sandwich", Description = "Triple-decker with turkey and bacon.", Price = 380, CategoryId = sandwiches.Id, ImageUrl = "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=400" },
            new() { Name = "Grilled Chicken", Description = "Tender grilled chicken with lettuce.", Price = 420, CategoryId = sandwiches.Id, ImageUrl = "https://images.unsplash.com/photo-1606755962773-d324e0a13086?w=400" },
            new() { Name = "BLT", Description = "Bacon, lettuce and tomato classic.", Price = 350, CategoryId = sandwiches.Id, ImageUrl = "https://images.unsplash.com/photo-1619096252214-ef06c45683e3?w=400" },
            new() { Name = "Turkey Avocado", Description = "Sliced turkey with creamy avocado.", Price = 480, CategoryId = sandwiches.Id, ImageUrl = "https://images.unsplash.com/photo-1481070414801-51fd732d7184?w=400" },
            new() { Name = "Alfredo", Description = "Creamy parmesan alfredo sauce.", Price = 520, CategoryId = pasta.Id, ImageUrl = "https://images.unsplash.com/photo-1645112411341-6c4fd023714a?w=400" },
            new() { Name = "Arrabbiata", Description = "Spicy tomato arrabbiata pasta.", Price = 480, CategoryId = pasta.Id, ImageUrl = "https://images.unsplash.com/photo-1608219992759-8d74ed8d76eb?w=400" },
            new() { Name = "Pesto Chicken", Description = "Basil pesto with grilled chicken.", Price = 580, CategoryId = pasta.Id, ImageUrl = "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400" },
            new() { Name = "Mac and Cheese", Description = "Creamy macaroni in cheese sauce.", Price = 420, CategoryId = pasta.Id, ImageUrl = "https://images.unsplash.com/photo-1543352634-a1c51d9f1fa7?w=400" },
            new() { Name = "Bolognese", Description = "Rich meat sauce over spaghetti.", Price = 550, CategoryId = pasta.Id, ImageUrl = "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400" },
            new() { Name = "Lasagna", Description = "Layered pasta with beef and cheese.", Price = 680, CategoryId = pasta.Id, ImageUrl = "https://images.unsplash.com/photo-1574894709920-11b28e7367e3?w=400" },
            new() { Name = "Mango Shake", Description = "Fresh mango blended smooth.", Price = 220, CategoryId = drinks.Id, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" },
            new() { Name = "Oreo Milkshake", Description = "Creamy Oreo cookie shake.", Price = 280, CategoryId = drinks.Id, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" },
            new() { Name = "Fresh Lemonade", Description = "Zesty homemade lemonade.", Price = 180, CategoryId = drinks.Id, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" },
            new() { Name = "Cold Coffee", Description = "Iced coffee with milk.", Price = 250, CategoryId = drinks.Id, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400" },
            new() { Name = "Student Deal", Description = "Burger + drink + fries combo.", Price = 650, CategoryId = deals.Id, ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400" },
            new() { Name = "Family Deal", Description = "2 pizzas + 4 drinks for the family.", Price = 2200, CategoryId = deals.Id, ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400" },
            new() { Name = "Lunch Special", Description = "Sandwich + drink lunch combo.", Price = 480, CategoryId = deals.Id, ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400" }
        };
    }

    private static List<User> CreateDefaultUsers() => new()
    {
        new User
        {
            Name = "Admin",
            Email = "admin@byteme.com",
            PasswordHash = PasswordHasher.Hash("Admin@123"),
            Role = "Admin"
        },
        new User
        {
            Name = "Test User",
            Email = "user@byteme.com",
            PasswordHash = PasswordHasher.Hash("User@123"),
            Role = "Customer"
        }
    };
}
