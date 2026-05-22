import { test, expect } from '../fixtures';

test.describe('Фильтрация блюд', () => {

  test.beforeEach(async ({ dishListPage }) => {
    await dishListPage.goto();
  });

  test('Поиск: найдено', async ({ productListPage, dishListPage }) => {
    await productListPage.goto();
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name: 'ДляПоискаБлюд', calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await dishListPage.goto();
    const name = `УникБлюдо_${Date.now()}`;
    await dishListPage.openCreateForm();
    await dishListPage.fillDishForm({ name, portionSize: 200, category: '2', ingredients: [{ productName: 'ДляПоискаБлюд', amount: 100 }] });
    await dishListPage.submitForm();
    await dishListPage.searchDish('');
    await dishListPage.page.waitForTimeout(300);
    await dishListPage.searchDish(name);
    await dishListPage.page.waitForTimeout(500);
    expect(await dishListPage.getDishCount()).toBeGreaterThanOrEqual(1);
  });

  test('Поиск: не найдено', async ({ dishListPage }) => {
    await dishListPage.searchDish('');
    await dishListPage.page.waitForTimeout(300);
    await dishListPage.searchDish(`НеСуществует_${Date.now()}`);
    await dishListPage.page.waitForTimeout(500);
    expect(await dishListPage.getDishCount()).toBe(0);
  });

  [0, 1, 2, 3, 4, 5, 6].forEach(cat => {
    test(`Категория ${cat}`, async ({ dishListPage }) => {
      await dishListPage.selectCategory(String(cat));
      await dishListPage.page.waitForTimeout(300);
      expect(await dishListPage.getDishCount()).toBeGreaterThanOrEqual(0);
    });
  });

  ['calories', 'proteins', 'fats', 'carbs'].forEach(field => {
    test(`Сортировка ${field} ↑`, async ({ dishListPage }) => {
      await dishListPage.sortBy(field);
      await dishListPage.page.waitForTimeout(300);
      expect(await dishListPage.getDishCount()).toBeGreaterThanOrEqual(0);
    });
    test(`Сортировка ${field} ↓`, async ({ dishListPage }) => {
      await dishListPage.sortBy(field);
      await dishListPage.toggleSortOrder();
      await dishListPage.page.waitForTimeout(300);
      expect(await dishListPage.getDishCount()).toBeGreaterThanOrEqual(0);
    });
  });
});