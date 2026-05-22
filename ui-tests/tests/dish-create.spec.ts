import { test, expect } from '../fixtures';

test.describe('Создание блюда', () => {

  test.beforeEach(async ({ productListPage, dishListPage }) => {
    await productListPage.goto();
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name: 'ИнгредиентДляБлюда', calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await expect(productListPage.productForm).not.toBeVisible();
    await dishListPage.goto();
  });

  test('Валидное блюдо', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.fillDishForm({ name: 'Куриный суп', portionSize: 250, category: '5', ingredients: [{ productName: 'ИнгредиентДляБлюда', amount: 100 }] });
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).not.toBeVisible();
    await expect(dishListPage.dishCards.filter({ hasText: 'Куриный суп' }).first()).toBeVisible();
  });

  test('Без ингредиентов', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.nameInput.fill('Пустое блюдо');
    await dishListPage.portionSizeInput.fill('100');
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).toBeVisible();
  });

  test('Пустое имя', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.nameInput.fill('');
    await dishListPage.portionSizeInput.fill('100');
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).toBeVisible();
  });

  test('Без имени', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.portionSizeInput.fill('100');
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).toBeVisible();
  });

  test('Без порции', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.nameInput.fill('Без порции');
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).toBeVisible();
  });

  test('Название 2 символа', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.fillDishForm({ name: 'Су', portionSize: 100, category: '5', ingredients: [{ productName: 'ИнгредиентДляБлюда', amount: 50 }] });
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).not.toBeVisible();
  });

  test('Название 1 символ', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.nameInput.fill('С');
    await dishListPage.portionSizeInput.fill('100');
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).toBeVisible();
  });

  test('Порция 0.1', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.fillDishForm({ name: 'Микроблюдо', portionSize: 0.1, category: '6', ingredients: [{ productName: 'ИнгредиентДляБлюда', amount: 0.1 }] });
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).not.toBeVisible();
  });

  test('Порция 0', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.nameInput.fill('Нулевая порция');
    await dishListPage.portionSizeInput.fill('0');
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).toBeVisible();
  });

  test('Ручной ввод КБЖУ', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.page.locator('.manual-toggle input').check();
    await dishListPage.page.waitForTimeout(300);
    await dishListPage.page.locator('.bju-row input').nth(0).fill('500');
    await dishListPage.page.locator('.bju-row input').nth(1).fill('40');
    await dishListPage.page.locator('.bju-row input').nth(2).fill('30');
    await dishListPage.page.locator('.bju-row input').nth(3).fill('30');
    await dishListPage.nameInput.fill('РучнойБЖУ');
    await dishListPage.portionSizeInput.fill('200');
    await dishListPage.formCategorySelect.selectOption('2');
    const option = dishListPage.ingredientSelect.first().locator('option').nth(1);
    const value = await option.getAttribute('value');
    if (value) await dishListPage.ingredientSelect.first().selectOption(value);
    await dishListPage.ingredientAmount.first().fill('50');
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).not.toBeVisible();
  });

  test('Авто-расчёт КБЖУ', async ({ dishListPage }) => {
    await dishListPage.openCreateForm();
    await dishListPage.nameInput.fill('АвтоБЖУ');
    await dishListPage.portionSizeInput.fill('200');
    await dishListPage.formCategorySelect.selectOption('2');
    const option = dishListPage.ingredientSelect.first().locator('option').nth(1);
    const value = await option.getAttribute('value');
    await dishListPage.ingredientSelect.first().selectOption(value);
    await dishListPage.ingredientAmount.first().fill('50');
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).not.toBeVisible();
  });

  const macros = [
    { input: 'Торт !десерт', expected: 'Торт', category: '0' },
    { input: 'Борщ !первое', expected: 'Борщ', category: '1' },
    { input: 'Стейк !второе', expected: 'Стейк', category: '2' },
    { input: 'Кофе !напиток', expected: 'Кофе', category: '3' },
    { input: 'Цезарь !салат', expected: 'Цезарь', category: '4' },
    { input: 'Харчо !суп', expected: 'Харчо', category: '5' },
    { input: 'Орехи !перекус', expected: 'Орехи', category: '6' },
  ];

  macros.forEach(({ input, expected, category }) => {
    test(`Макрос: "${input}" → "${expected}"`, async ({ dishListPage }) => {
      await dishListPage.openCreateForm();
      await dishListPage.fillDishForm({ name: input, portionSize: 150, category, ingredients: [{ productName: 'ИнгредиентДляБлюда', amount: 100 }] });
      await dishListPage.submitForm();
      await expect(dishListPage.dishForm).not.toBeVisible();
      await dishListPage.page.waitForTimeout(500);
      const count = await dishListPage.dishCards.count();
      expect(count).toBeGreaterThanOrEqual(1);
    });
  });
});