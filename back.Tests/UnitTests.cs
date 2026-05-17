using back.Utils;
using back.Models;

namespace back.Tests;

public class DishCalculatorTests
{
    private static Product CreateProduct(
        string name = "Тестовый продукт",
        double calories = 0,
        double proteins = 0,
        double fats = 0,
        double carbs = 0)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Calories = calories,
            Proteins = proteins,
            Fats = fats,
            Carbohydrates = carbs,
            Category = Enums.ProductCategory.Vegetables,
            Readiness = Enums.Readiness.ReadyToEat
        };
    }

    private static Dish CreateDish(List<DishesProduct> ingredients)
    {
        return new Dish
        {
            Id = Guid.NewGuid(),
            Name = "Тестовое блюдо",
            Calories = 0,
            Proteins = 0,
            Fats = 0,
            Carbohydrates = 0,
            PortionSize = 200,
            Category = Enums.DishCategory.Second,
            DishesProducts = ingredients
        };
    }

    private static DishesProduct CreateIngredient(Product product, double amount)
    {
        return new DishesProduct
        {
            Id = Guid.NewGuid(),
            Product = product,
            Amount = amount
        };
    }

    [Fact]
    public void Calculate_EmptyDish_ReturnsAllZeros()
    {
        var dish = CreateDish(new List<DishesProduct>());

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(0, calories);
        Assert.Equal(0, proteins);
        Assert.Equal(0, fats);
        Assert.Equal(0, carbs);
    }

    [Fact]
    public void Calculate_SingleProduct_100g_ReturnsExactValues()
    {
        var product = CreateProduct(calories: 250, proteins: 10, fats: 5, carbs: 30);
        var dish = CreateDish(new List<DishesProduct> { CreateIngredient(product, 100) });

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(250, calories);
        Assert.Equal(10, proteins);
        Assert.Equal(5, fats);
        Assert.Equal(30, carbs);
    }

    [Fact]
    public void Calculate_MultipleProducts_ReturnsSum()
    {
        var p1 = CreateProduct(calories: 100, proteins: 5, fats: 2, carbs: 10);
        var p2 = CreateProduct(calories: 200, proteins: 15, fats: 8, carbs: 20);
        var p3 = CreateProduct(calories: 50, proteins: 1, fats: 0.5, carbs: 5);
        var dish = CreateDish(new List<DishesProduct>
        {
            CreateIngredient(p1, 100),
            CreateIngredient(p2, 50),
            CreateIngredient(p3, 200),
        });

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(300, calories);
        Assert.Equal(14.5, proteins, precision: 5);
        Assert.Equal(7, fats);
        Assert.Equal(30, carbs);
    }

    [Fact]
    public void Calculate_ZeroAmount_ReturnsZero()
    {
        var product = CreateProduct(calories: 500, proteins: 20, fats: 10, carbs: 50);
        var dish = CreateDish(new List<DishesProduct> { CreateIngredient(product, 0) });

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(0, calories);
        Assert.Equal(0, proteins);
        Assert.Equal(0, fats);
        Assert.Equal(0, carbs);
    }

    [Fact]
    public void Calculate_NullProduct_SkippedGracefully()
    {
        var product = CreateProduct(calories: 100, proteins: 5, fats: 2, carbs: 10);
        var nullIngredient = new DishesProduct
        {
            Id = Guid.NewGuid(),
            Product = null,
            Amount = 100
        };
        var dish = CreateDish(new List<DishesProduct>
        {
            nullIngredient,
            CreateIngredient(product, 100)
        });

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(100, calories);
        Assert.Equal(5, proteins);
        Assert.Equal(2, fats);
        Assert.Equal(10, carbs);
    }

    [Theory]
    [InlineData(100, 250, 10, 5, 30)]
    [InlineData(200, 500, 20, 10, 60)]    
    [InlineData(50, 125, 5, 2.5, 15)]           
    [InlineData(75, 187.5, 7.5, 3.75, 22.5)]   
    [InlineData(0, 0, 0, 0, 0)]            
    public void Calculate_VariousAmounts_CorrectFactor(
        double amount,
        double expectedCals,
        double expectedProt,
        double expectedFat,
        double expectedCarbs)
    {
        var product = CreateProduct(calories: 250, proteins: 10, fats: 5, carbs: 30);
        var dish = CreateDish(new List<DishesProduct> { CreateIngredient(product, amount) });

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(expectedCals, calories, precision: 5);
        Assert.Equal(expectedProt, proteins, precision: 5);
        Assert.Equal(expectedFat, fats, precision: 5);
        Assert.Equal(expectedCarbs, carbs, precision: 5);
    }

    [Theory]
    [InlineData(-0.1, -0.1, -0.01, -0.005, -0.02)]
    [InlineData(0.1, 0.1, 0.01, 0.005, 0.02)]   
    [InlineData(1, 1, 0.1, 0.05, 0.2)]               
    [InlineData(10, 10, 1, 0.5, 2)]                  
    [InlineData(100, 100, 10, 5, 20)]                
    [InlineData(200, 200, 20, 10, 40)]        
    [InlineData(1000, 1000, 100, 50, 200)]        
    [InlineData(10_000, 10_000, 1_000, 500, 2_000)]
    public void Calculate_BoundaryAmounts(
        double amount,
        double expectedCals,
        double expectedProt,
        double expectedFat,
        double expectedCarbs)
    {
        var product = CreateProduct(calories: 100, proteins: 10, fats: 5, carbs: 20);
        var dish = CreateDish(new List<DishesProduct> { CreateIngredient(product, amount) });

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(expectedCals, calories, precision: 5);
        Assert.Equal(expectedProt, proteins, precision: 5);
        Assert.Equal(expectedFat, fats, precision: 5);
        Assert.Equal(expectedCarbs, carbs, precision: 5);
    }

    [Theory]
    [InlineData(-100, 10, 5, 20, 100, -100, 10, 5, 20)] 
    [InlineData(100, 10, 5, 20, -50, -50, -5, -2.5, -10)]   
    [InlineData(-200, -15, -8, -30, 50, -100, -7.5, -4, -15)] 
    public void Calculate_NegativeValues(
        double cal, double prot, double fat, double carb,
        double amount,
        double expectedCals, double expectedProt, double expectedFat, double expectedCarbs)
    {
        var product = CreateProduct(calories: cal, proteins: prot, fats: fat, carbs: carb);
        var dish = CreateDish(new List<DishesProduct> { CreateIngredient(product, amount) });

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(expectedCals, calories, precision: 5);
        Assert.Equal(expectedProt, proteins, precision: 5);
        Assert.Equal(expectedFat, fats, precision: 5);
        Assert.Equal(expectedCarbs, carbs, precision: 5);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 100, 0, 0, 0, 0)]                        
    [InlineData(500, 0, 0, 0, 200, 1000, 0, 0, 0)]                    
    [InlineData(900, 100, 100, 100, 50, 450, 50, 50, 50)]       
    [InlineData(200, 15, 8, 30, 150, 300, 22.5, 12, 45)]    
    public void Calculate_BoundaryNutrients(
        double cal, double prot, double fat, double carb,
        double amount,
        double expectedCals, double expectedProt, double expectedFat, double expectedCarbs)
    {
        var product = CreateProduct(calories: cal, proteins: prot, fats: fat, carbs: carb);
        var dish = CreateDish(new List<DishesProduct> { CreateIngredient(product, amount) });

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(expectedCals, calories, precision: 5);
        Assert.Equal(expectedProt, proteins, precision: 5);
        Assert.Equal(expectedFat, fats, precision: 5);
        Assert.Equal(expectedCarbs, carbs, precision: 5);
    }


    [Fact]
    public void Calculate_MixedZeroFractionalAndNormal()
    {
        var p1 = CreateProduct(calories: 200, proteins: 10, fats: 8, carbs: 25);
        var p2 = CreateProduct(calories: 150, proteins: 5, fats: 3, carbs: 15);
        var p3 = CreateProduct(calories: 300, proteins: 20, fats: 10, carbs: 30);

        var dish = CreateDish(new List<DishesProduct>
        {
            CreateIngredient(p1, 75),  
            CreateIngredient(p2, 0),  
            CreateIngredient(p3, 33.3), 
        });

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(249.9, calories, precision: 2);
        Assert.Equal(14.16, proteins, precision: 2);
        Assert.Equal(9.33, fats, precision: 2);
        Assert.Equal(28.74, carbs, precision: 2);
    }

    [Fact]
    public void Calculate_TenProducts_10gEach()
    {
        var product = CreateProduct(calories: 50, proteins: 3, fats: 1, carbs: 8);
        var ingredients = Enumerable.Range(0, 10)
            .Select(_ => CreateIngredient(product, 10))
            .ToList();

        var dish = CreateDish(ingredients);

        var (calories, proteins, fats, carbs) = DishCalculator.Calculate(dish);

        Assert.Equal(50, calories, precision: 5);
        Assert.Equal(3, proteins, precision: 5);
        Assert.Equal(1, fats, precision: 5);
        Assert.Equal(8, carbs, precision: 5);
    }
}