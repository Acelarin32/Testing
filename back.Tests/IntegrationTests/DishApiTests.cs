using System.Net;
using System.Text.Json;
using back.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace back.Tests.IntegrationTests;

public class DishApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DishApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private async Task<Guid> CreateTestProduct(
        string name = "Тестовый продукт",
        double calories = 100,
        double proteins = 10,
        double fats = 5,
        double carbs = 20,
        int dietaryFlags = 0,
        int category = 0,
        int readiness = 0)
    {
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(name), "Name");
        formData.Add(new StringContent(calories.ToString()), "Calories");
        formData.Add(new StringContent(proteins.ToString()), "Proteins");
        formData.Add(new StringContent(fats.ToString()), "Fats");
        formData.Add(new StringContent(carbs.ToString()), "Carbohydrates");
        formData.Add(new StringContent(category.ToString()), "Category");
        formData.Add(new StringContent(readiness.ToString()), "Readiness");
        formData.Add(new StringContent(dietaryFlags.ToString()), "DietaryFlags");

        var response = await _client.PostAsync("/api/product", formData);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(json, _jsonOptions);
        return product!.Id;
    }

    private MultipartFormDataContent CreateDishFormData(
        string name,
        double portionSize,
        int category,
        Guid productId,
        double amount,
        double? calories = null,
        double? proteins = null,
        double? fats = null,
        double? carbohydrates = null)
    {
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(name), "Name");
        formData.Add(new StringContent(portionSize.ToString()), "PortionSize");
        formData.Add(new StringContent(category.ToString()), "Category");
        formData.Add(new StringContent(productId.ToString()), "Ingredients[0].ProductId");
        formData.Add(new StringContent(amount.ToString()), "Ingredients[0].Amount");

        if (calories.HasValue)
            formData.Add(new StringContent(calories.Value.ToString()), "Calories");
        if (proteins.HasValue)
            formData.Add(new StringContent(proteins.Value.ToString()), "Proteins");
        if (fats.HasValue)
            formData.Add(new StringContent(fats.Value.ToString()), "Fats");
        if (carbohydrates.HasValue)
            formData.Add(new StringContent(carbohydrates.Value.ToString()), "Carbohydrates");

        return formData;
    }

    [Fact]
    public async Task CreateDish_ValidData_ReturnsCreated()
    {
        var productId = await CreateTestProduct("Курица", calories: 100, proteins: 10, fats: 5, carbs: 20);
        var formData = CreateDishFormData("Куриное филе", 200, 2, productId, 100);

        var response = await _client.PostAsync("/api/dish", formData);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var dish = JsonSerializer.Deserialize<Dish>(json, _jsonOptions);

        Assert.NotNull(dish);
        Assert.Equal("Куриное филе", dish!.Name);
        Assert.Equal(200, dish.PortionSize);
        Assert.Equal(100, dish.Calories, precision: 2);
        Assert.Equal(10, dish.Proteins, precision: 2);
        Assert.Equal(5, dish.Fats, precision: 2);
        Assert.Equal(20, dish.Carbohydrates, precision: 2);
        Assert.Single(dish.DishesProducts);
    }

    [Fact]
    public async Task CreateDish_NoIngredients_ReturnsBadRequest()
    {
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("Пустое блюдо"), "Name");
        formData.Add(new StringContent("100"), "PortionSize");
        formData.Add(new StringContent("2"), "Category");

        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_NonExistentProduct_ReturnsBadRequest()
    {
        var formData = CreateDishFormData("Несуществующий", 100, 4, Guid.NewGuid(), 50);

        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("не найден", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateDish_EmptyBody_ReturnsBadRequest()
    {
        var formData = new MultipartFormDataContent();
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_EmptyName_ReturnsBadRequest()
    {
        var productId = await CreateTestProduct("Для теста");
        var formData = CreateDishFormData("", 100, 2, productId, 50);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_NoName_ReturnsBadRequest()
    {
        var productId = await CreateTestProduct("Для теста");
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("100"), "PortionSize");
        formData.Add(new StringContent("2"), "Category");
        formData.Add(new StringContent(productId.ToString()), "Ingredients[0].ProductId");
        formData.Add(new StringContent("50"), "Ingredients[0].Amount");

        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_NoPortionSize_ReturnsBadRequest()
    {
        var productId = await CreateTestProduct("Для теста");
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("Без порции"), "Name");
        formData.Add(new StringContent("2"), "Category");
        formData.Add(new StringContent(productId.ToString()), "Ingredients[0].ProductId");
        formData.Add(new StringContent("50"), "Ingredients[0].Amount");

        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_NameExactly2Chars_ReturnsCreated()
    {
        var productId = await CreateTestProduct("Соль");
        var formData = CreateDishFormData("Су", 100, 5, productId, 10);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_NameTooShort_ReturnsBadRequest()
    {
        var productId = await CreateTestProduct("Соль");
        var formData = CreateDishFormData("С", 100, 5, productId, 10);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_NameExactly200Chars_ReturnsCreated()
    {
        var productId = await CreateTestProduct("Вода");
        var longName = new string('А', 200);
        var formData = CreateDishFormData(longName, 100, 3, productId, 100);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_NameTooLong_ReturnsBadRequest()
    {
        var productId = await CreateTestProduct("Вода");
        var tooLongName = new string('А', 201);
        var formData = CreateDishFormData(tooLongName, 100, 3, productId, 100);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_MinimalPortionSize_ReturnsCreated()
    {
        var productId = await CreateTestProduct("Микро");
        var formData = CreateDishFormData("Микропорция", 0.1, 6, productId, 0.1);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_ZeroPortionSize_ReturnsBadRequest()
    {
        var productId = await CreateTestProduct("Вода");
        var formData = CreateDishFormData("Нулевая порция", 0, 3, productId, 50);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_VeryLargePortion_ReturnsCreated()
    {
        var productId = await CreateTestProduct("Гигант");
        var formData = CreateDishFormData("Гигапорция", 999_999, 2, productId, 1000);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_ManyIngredients_ReturnsCreated()
    {
        var productId = await CreateTestProduct("База");
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("Многокомпонентное"), "Name");
        formData.Add(new StringContent("500"), "PortionSize");
        formData.Add(new StringContent("2"), "Category");

        for (int i = 0; i < 20; i++)
        {
            formData.Add(new StringContent(productId.ToString()), $"Ingredients[{i}].ProductId");
            formData.Add(new StringContent("10"), $"Ingredients[{i}].Amount");
        }

        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_AutoCalculateBju_ReturnsCalculatedValues()
    {
        var productId = await CreateTestProduct("Авто-продукт", calories: 200, proteins: 15, fats: 8, carbs: 25);
        var formData = CreateDishFormData("Авто-расчёт", 250, 2, productId, 50);

        var response = await _client.PostAsync("/api/dish", formData);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var dish = JsonSerializer.Deserialize<Dish>(json, _jsonOptions);
        Assert.Equal(100, dish!.Calories, precision: 2);
        Assert.Equal(7.5, dish.Proteins, precision: 2);
        Assert.Equal(4, dish.Fats, precision: 2);
        Assert.Equal(12.5, dish.Carbohydrates, precision: 2);
    }

    [Fact]
    public async Task CreateDish_ManualBju_OverridesAutoCalculation()
    {
        var productId = await CreateTestProduct("Для ручного ввода");
        var formData = CreateDishFormData("Ручной ввод", 200, 2, productId, 100,
            calories: 500, proteins: 50, fats: 30, carbohydrates: 20);

        var response = await _client.PostAsync("/api/dish", formData);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var dish = JsonSerializer.Deserialize<Dish>(json, _jsonOptions);
        Assert.Equal(500, dish!.Calories);
        Assert.Equal(50, dish.Proteins);
        Assert.Equal(30, dish.Fats);
        Assert.Equal(20, dish.Carbohydrates);
    }

    [Fact]
    public async Task CreateDish_BjuPer100gExceeds100_ReturnsBadRequest()
    {
        var productId = await CreateTestProduct("Умеренный", proteins: 25, fats: 25, carbs: 25);
        var formData = CreateDishFormData("Перебор БЖУ", 50, 2, productId, 200);

        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("Торт !десерт", "Торт", 0)]
    [InlineData("Борщ !первое", "Борщ", 1)]
    [InlineData("Стейк !второе", "Стейк", 2)]
    [InlineData("Кофе !напиток", "Кофе", 3)]
    [InlineData("Цезарь !салат", "Цезарь", 4)]
    [InlineData("Харчо !суп", "Харчо", 5)]
    [InlineData("Орехи !перекус", "Орехи", 6)]
    public async Task CreateDish_AllMacros_SetsCategoryAndRemovesMacro(
        string inputName, string expectedName, int expectedCategory)
    {
        var productId = await CreateTestProduct($"Ингредиент_{expectedCategory}");
        var formData = CreateDishFormData(inputName, 150, expectedCategory, productId, 100);

        var response = await _client.PostAsync("/api/dish", formData);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var dish = JsonSerializer.Deserialize<Dish>(json, _jsonOptions);
        Assert.Equal(expectedName, dish!.Name);
        Assert.Equal(expectedCategory, (int)dish.Category);
    }

    [Fact]
    public async Task GetDishList_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/dish");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDishById_AfterCreate_ReturnsDishWithIngredients()
    {
        var productId = await CreateTestProduct("Для получения");
        var formData = CreateDishFormData("Блюдо для GET", 300, 1, productId, 150);
        var createResponse = await _client.PostAsync("/api/dish", formData);
        var json = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<Dish>(json, _jsonOptions);

        var response = await _client.GetAsync($"/api/dish/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var getJson = await response.Content.ReadAsStringAsync();
        var dish = JsonSerializer.Deserialize<Dish>(getJson, _jsonOptions);
        Assert.Equal("Блюдо для GET", dish!.Name);
        Assert.Equal(300, dish.PortionSize);
        Assert.NotEmpty(dish.DishesProducts);
    }

    [Fact]
    public async Task GetDishById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/dish/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDish_AllFields_ReturnsUpdated()
    {
        var productId = await CreateTestProduct("Для обновления блюда");
        var createForm = CreateDishFormData("До обновления", 200, 4, productId, 100);
        var createResponse = await _client.PostAsync("/api/dish", createForm);
        var json = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<Dish>(json, _jsonOptions);

        var updateForm = new MultipartFormDataContent();
        updateForm.Add(new StringContent("После обновления"), "Name");
        updateForm.Add(new StringContent("350"), "PortionSize");
        updateForm.Add(new StringContent("5"), "Category");
        updateForm.Add(new StringContent("500"), "Calories");
        updateForm.Add(new StringContent("40"), "Proteins");
        updateForm.Add(new StringContent("30"), "Fats");
        updateForm.Add(new StringContent("50"), "Carbohydrates");
        updateForm.Add(new StringContent(productId.ToString()), "Ingredients[0].ProductId");
        updateForm.Add(new StringContent("80"), "Ingredients[0].Amount");

        var updateResponse = await _client.PutAsync($"/api/dish/{created!.Id}", updateForm);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedJson = await updateResponse.Content.ReadAsStringAsync();
        var updated = JsonSerializer.Deserialize<Dish>(updatedJson, _jsonOptions);
        Assert.Equal("После обновления", updated!.Name);
        Assert.Equal(350, updated.PortionSize);
        Assert.Equal(5, (int)updated.Category);
        Assert.Equal(500, updated.Calories, precision: 2);
        Assert.Equal(40, updated.Proteins, precision: 2);
        Assert.Equal(30, updated.Fats, precision: 2);
        Assert.Equal(50, updated.Carbohydrates, precision: 2);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateDish_NonExistent_ReturnsNotFound()
    {
        var productId = await CreateTestProduct("Любой");
        var formData = CreateDishFormData("Неважно", 100, 2, productId, 50);
        var response = await _client.PutAsync($"/api/dish/{Guid.NewGuid()}", formData);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateThenDelete_DishRemoved()
    {
        var productId = await CreateTestProduct("Для удаления");
        var formData = CreateDishFormData("Блюдо на удаление", 150, 5, productId, 50);

        var createResponse = await _client.PostAsync("/api/dish", formData);
        var json = await createResponse.Content.ReadAsStringAsync();
        var dish = JsonSerializer.Deserialize<Dish>(json, _jsonOptions);

        var deleteResponse = await _client.DeleteAsync($"/api/dish/{dish!.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/dish/{dish.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public async Task GetDishList_ByAllCategories_ReturnsOk(int category)
    {
        var productId = await CreateTestProduct($"Ингр_{category}");
        var formData = CreateDishFormData($"Блюдо_{category}", 100, category, productId, 50);
        await _client.PostAsync("/api/dish", formData);

        var response = await _client.GetAsync($"/api/dish?category={category}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public async Task GetDishList_ByDietaryFlag_AllSingle_ReturnsOk(int flag)
    {
        var productId = await CreateTestProduct($"Продукт_{flag}", dietaryFlags: flag);
        var formData = CreateDishFormData($"Блюдо_{flag}", 100, 2, productId, 50);
        await _client.PostAsync("/api/dish", formData);

        var response = await _client.GetAsync($"/api/dish?dietaryFlags={flag}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDishList_BySearch_Found_ReturnsFiltered()
    {
        var productId = await CreateTestProduct("Для поиска");
        var uniqueName = $"ПоискБлюдо_{Guid.NewGuid():N}";
        var formData = CreateDishFormData(uniqueName, 100, 5, productId, 50);
        await _client.PostAsync("/api/dish", formData);

        var response = await _client.GetAsync($"/api/dish?search={uniqueName}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var dishes = JsonSerializer.Deserialize<List<Dish>>(json, _jsonOptions);
        Assert.NotEmpty(dishes!);
        Assert.All(dishes!, d => Assert.Contains(uniqueName, d.Name));
    }

    [Fact]
    public async Task GetDishList_BySearch_NotFound_ReturnsEmptyList()
    {
        var nonexistentName = $"НеСуществует_{Guid.NewGuid():N}";

        var response = await _client.GetAsync($"/api/dish?search={nonexistentName}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var dishes = JsonSerializer.Deserialize<List<Dish>>(json, _jsonOptions);
        Assert.Empty(dishes!);
    }

    [Theory]
    [InlineData("name", true)]
    [InlineData("name", false)]
    [InlineData("calories", true)]
    [InlineData("calories", false)]
    [InlineData("proteins", true)]
    [InlineData("proteins", false)]
    [InlineData("fats", true)]
    [InlineData("fats", false)]
    [InlineData("carbs", true)]
    [InlineData("carbs", false)]
    public async Task GetDishList_SortByAllFields_BothDirections_ReturnsOk(string sortBy, bool ascending)
    {
        var response = await _client.GetAsync($"/api/dish?sortBy={sortBy}&ascending={ascending}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_NegativeAmount_ReturnsBadRequest()
    {
        var productId = await CreateTestProduct("ОтрицКоличество");
        var formData = CreateDishFormData("Отрицательное количество", 100, 2, productId, -50);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_NegativeAmountNearZero_ReturnsBadRequest()
    {
        var productId = await CreateTestProduct("ОколоНуля");
        var formData = CreateDishFormData("Около нуля", 100, 2, productId, -0.1);
        var response = await _client.PostAsync("/api/dish", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}