import { test, expect } from '../fixtures';

test.describe('Просмотр блюда', () => {
  test.beforeEach(async ({ productListPage }) => {
    await productListPage.goto();
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name: 'ДляПросмотра', calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
  });

  test('Клик по карточке открывает модал', async ({ dishListPage }) => {
    await dishListPage.goto();
    await dishListPage.openCreateForm();
    await dishListPage.fillDishForm({ name: 'ПросмотрБлюда', portionSize: 200, category: '2', ingredients: [{ productName: 'ДляПросмотра', amount: 100 }] });
    await dishListPage.submitForm();
    await dishListPage.dishCards.filter({ hasText: 'ПросмотрБлюда' }).first().click();
    await expect(dishListPage.page.locator('.modal-content')).toBeVisible();
  });
});