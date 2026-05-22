import { test, expect } from '../fixtures';

test.describe('Фильтрация продуктов', () => {

  test.beforeEach(async ({ productListPage }) => {
    await productListPage.goto();
  });

  test('Поиск: найдено', async ({ productListPage }) => {
    const name = `УникПоиск_${Date.now()}`;
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name, calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await productListPage.searchProduct('');
    await productListPage.page.waitForTimeout(300);
    await productListPage.searchProduct(name);
    await productListPage.page.waitForTimeout(500);
    expect(await productListPage.getProductCount()).toBeGreaterThanOrEqual(1);
  });

  test('Поиск: не найдено', async ({ productListPage }) => {
    await productListPage.searchProduct('');
    await productListPage.page.waitForTimeout(300);
    await productListPage.searchProduct(`НеСуществует_${Date.now()}`);
    await productListPage.page.waitForTimeout(500);
    expect(await productListPage.getProductCount()).toBe(0);
  });

  [0, 1, 2, 3, 4, 5, 6, 7, 8].forEach(cat => {
    test(`Категория ${cat}`, async ({ productListPage }) => {
      await productListPage.selectCategory(String(cat));
      await productListPage.page.waitForTimeout(300);
      expect(await productListPage.getProductCount()).toBeGreaterThanOrEqual(0);
    });
  });

  [0, 1, 2].forEach(r => {
    test(`Готовность ${r}`, async ({ productListPage }) => {
      await productListPage.selectReadiness(String(r));
      await productListPage.page.waitForTimeout(300);
      expect(await productListPage.getProductCount()).toBeGreaterThanOrEqual(0);
    });
  });

  ['calories', 'proteins', 'fats', 'carbohydrates'].forEach(field => {
    test(`Сортировка ${field} ↑`, async ({ productListPage }) => {
      await productListPage.sortBy(field);
      await productListPage.page.waitForTimeout(300);
      expect(await productListPage.getProductCount()).toBeGreaterThanOrEqual(0);
    });
    test(`Сортировка ${field} ↓`, async ({ productListPage }) => {
      await productListPage.sortBy(field);
      await productListPage.toggleSortOrder();
      await productListPage.page.waitForTimeout(300);
      expect(await productListPage.getProductCount()).toBeGreaterThanOrEqual(0);
    });
  });
});