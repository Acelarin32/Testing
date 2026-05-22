using System.Net;
using System.Text.Json;
using back.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace back.Tests.IntegrationTests;

public class ProductApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private MultipartFormDataContent CreateProductFormData(
        string name,
        double calories = 100,
        double proteins = 10,
        double fats = 5,
        double carbs = 20,
        int category = 0,
        int readiness = 0,
        int dietaryFlags = 0)
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
        return formData;
    }

    [Fact]
    public async Task CreateProduct_ValidData_ReturnsOk()
    {
        var formData = CreateProductFormData("Помидор");
        var response = await _client.PostAsync("/api/product", formData);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(json, _jsonOptions);

        Assert.NotNull(product);
        Assert.Equal("Помидор", product.Name);
        Assert.Equal(100, product.Calories);
        Assert.Equal(10, product.Proteins);
        Assert.Equal(5, product.Fats);
        Assert.Equal(20, product.Carbohydrates);
    }

    [Fact]
    public async Task CreateProduct_NoName_ReturnsBadRequest()
    {
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("100"), "Calories");
        formData.Add(new StringContent("10"), "Proteins");
        formData.Add(new StringContent("5"), "Fats");
        formData.Add(new StringContent("20"), "Carbohydrates");
        formData.Add(new StringContent("0"), "Category");
        formData.Add(new StringContent("0"), "Readiness");

        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_EmptyBody_ReturnsBadRequest()
    {
        var formData = new MultipartFormDataContent();
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_EmptyName_ReturnsBadRequest()
    {
        var formData = CreateProductFormData("");
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_BjuSumExactly100_ReturnsOk()
    {
        var formData = CreateProductFormData("Протеин", proteins: 50, fats: 30, carbs: 20);
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_BjuSumExceeds100_ReturnsBadRequest()
    {
        var formData = CreateProductFormData("Жирный", proteins: 50, fats: 31, carbs: 20);
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_NameExactly2Chars_ReturnsOk()
    {
        var formData = CreateProductFormData("Ук");
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_NameTooShort_ReturnsBadRequest()
    {
        var formData = CreateProductFormData("У");
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ZeroCalories_ReturnsOk()
    {
        var formData = CreateProductFormData("Вода", calories: 0, proteins: 0, fats: 0, carbs: 0);
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AllDietaryFlags_ReturnsOk()
    {
        var formData = CreateProductFormData("Суперфуд", dietaryFlags: 7);
        var response = await _client.PostAsync("/api/product", formData);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(json, _jsonOptions);
        Assert.Equal(7, (int)product!.DietaryFlags);
    }

    [Fact]
    public async Task GetProductList_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/product");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_NonExistent_ReturnsNotFound()
    {
        var fakeId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/product/{fakeId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_AllFields_ReturnsUpdated()
    {
        var createForm = CreateProductFormData("До обновления");
        var createResponse = await _client.PostAsync("/api/product", createForm);
        var json = await createResponse.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(json, _jsonOptions);

        var updateForm = new MultipartFormDataContent();
        updateForm.Add(new StringContent("После обновления"), "Name");
        updateForm.Add(new StringContent("200"), "Calories");
        updateForm.Add(new StringContent("15"), "Proteins");
        updateForm.Add(new StringContent("8"), "Fats");
        updateForm.Add(new StringContent("25"), "Carbohydrates");
        updateForm.Add(new StringContent("Супер состав"), "Composition");
        updateForm.Add(new StringContent("1"), "Category");
        updateForm.Add(new StringContent("1"), "Readiness");
        updateForm.Add(new StringContent("3"), "DietaryFlags");

        var updateResponse = await _client.PutAsync($"/api/product/{product!.Id}", updateForm);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedJson = await updateResponse.Content.ReadAsStringAsync();
        var updated = JsonSerializer.Deserialize<Product>(updatedJson, _jsonOptions);
        Assert.Equal("После обновления", updated!.Name);
        Assert.Equal(200, updated.Calories);
        Assert.Equal(15, updated.Proteins);
        Assert.Equal(8, updated.Fats);
        Assert.Equal(25, updated.Carbohydrates);
        Assert.Equal("Супер состав", updated.Composition);
        Assert.Equal(1, (int)updated.Category);
        Assert.Equal(1, (int)updated.Readiness);
        Assert.Equal(3, (int)updated.DietaryFlags);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateProduct_NonExistent_ReturnsNotFound()
    {
        var formData = CreateProductFormData("Неважно");
        var response = await _client.PutAsync($"/api/product/{Guid.NewGuid()}", formData);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_NotUsedInDishes_ReturnsOk()
    {
        var formData = CreateProductFormData("На удаление");
        var createResponse = await _client.PostAsync("/api/product", formData);
        var json = await createResponse.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(json, _jsonOptions);

        var deleteResponse = await _client.DeleteAsync($"/api/product/{product!.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_UsedInDish_ReturnsBadRequest()
    {
        var createForm = CreateProductFormData("В блюде");
        var createResponse = await _client.PostAsync("/api/product", createForm);
        var json = await createResponse.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(json, _jsonOptions);

        var dishForm = new MultipartFormDataContent();
        dishForm.Add(new StringContent("Блюдо с продуктом"), "Name");
        dishForm.Add(new StringContent("100"), "PortionSize");
        dishForm.Add(new StringContent("2"), "Category");
        dishForm.Add(new StringContent(product!.Id.ToString()), "Ingredients[0].ProductId");
        dishForm.Add(new StringContent("50"), "Ingredients[0].Amount");
        await _client.PostAsync("/api/dish", dishForm);

        var deleteResponse = await _client.DeleteAsync($"/api/product/{product.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
        var errorJson = await deleteResponse.Content.ReadAsStringAsync();
        Assert.Contains("используется", errorJson, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public async Task FilterProducts_ByCategory_AllCategories_ReturnsOk(int category)
    {
        var formData = CreateProductFormData($"Продукт_{category}", category: category);
        await _client.PostAsync("/api/product", formData);

        var response = await _client.GetAsync($"/api/product/filter?category={category}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task FilterProducts_ByReadiness_AllValues_ReturnsOk(int readiness)
    {
        var response = await _client.GetAsync($"/api/product/filter?readiness={readiness}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task FilterProducts_ByDietaryFlag_AllCombinations_ReturnsOk(int flag)
    {
        var formData = CreateProductFormData($"Продукт_{flag}", dietaryFlags: flag);
        await _client.PostAsync("/api/product", formData);

        var response = await _client.GetAsync($"/api/product/filter?dietaryFlags={flag}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FilterProducts_BySearch_Found_ReturnsFiltered()
    {
        var uniqueName = $"УникПродукт_{Guid.NewGuid():N}";
        var formData = CreateProductFormData(uniqueName);
        await _client.PostAsync("/api/product", formData);

        var response = await _client.GetAsync($"/api/product/filter?search={uniqueName}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions);
        Assert.NotEmpty(products!);
        Assert.All(products!, p => Assert.Contains(uniqueName, p.Name));
    }

    [Fact]
    public async Task FilterProducts_BySearch_NotFound_ReturnsEmptyList()
    {
        var nonexistentName = $"НеСуществует_{Guid.NewGuid():N}";

        var response = await _client.GetAsync($"/api/product/filter?search={nonexistentName}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions);
        Assert.Empty(products!);
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
    [InlineData("carbohydrates", true)]
    [InlineData("carbohydrates", false)]
    public async Task FilterProducts_SortByAllFields_BothDirections_ReturnsOk(string sortBy, bool ascending)
    {
        var response = await _client.GetAsync($"/api/product/filter?sortBy={sortBy}&ascending={ascending}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FilterProducts_SortByCalories_Ascending_IsSorted()
    {
        var response = await _client.GetAsync("/api/product/filter?sortBy=calories&ascending=true");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions);
        Assert.NotNull(products);
        Assert.True(products!.Count >= 2, "Для проверки сортировки нужно минимум 2 продукта");

        for (int i = 0; i < products.Count - 1; i++)
            Assert.True(products[i].Calories <= products[i + 1].Calories,
                $"Нарушен порядок: products[{i}].Calories={products[i].Calories} > products[{i + 1}].Calories={products[i + 1].Calories}");
    }

    [Fact]
    public async Task FilterProducts_SortByCalories_Descending_IsSorted()
    {
        var response = await _client.GetAsync("/api/product/filter?sortBy=calories&ascending=false");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions);
        Assert.NotNull(products);
        Assert.True(products!.Count >= 2, "Для проверки сортировки нужно минимум 2 продукта");

        for (int i = 0; i < products.Count - 1; i++)
            Assert.True(products[i].Calories >= products[i + 1].Calories,
                $"Нарушен порядок: products[{i}].Calories={products[i].Calories} < products[{i + 1}].Calories={products[i + 1].Calories}");
    }

    [Fact]
    public async Task DeleteProduct_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/product/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_NegativeCalories_ReturnsBadRequest()
    {
        var formData = CreateProductFormData("ОтрицКалории", calories: -100, proteins: 10, fats: 5, carbs: 20);
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_NegativeProteins_ReturnsBadRequest()
    {
        var formData = CreateProductFormData("ОтрицБелки", calories: 100, proteins: -10, fats: 5, carbs: 20);
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_NegativeFats_ReturnsBadRequest()
    {
        var formData = CreateProductFormData("ОтрицЖиры", calories: 100, proteins: 10, fats: -5, carbs: 20);
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_NegativeCarbohydrates_ReturnsBadRequest()
    {
        var formData = CreateProductFormData("ОтрицУглеводы", calories: 100, proteins: 10, fats: 5, carbs: -20);
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AllNegative_ReturnsBadRequest()
    {
        var formData = CreateProductFormData("ВсёОтриц", calories: -200, proteins: -15, fats: -8, carbs: -30);
        var response = await _client.PostAsync("/api/product", formData);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}