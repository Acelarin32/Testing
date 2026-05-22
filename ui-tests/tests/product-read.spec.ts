import { test, expect } from '../fixtures';

test.describe('Просмотр продукта', () => {
  test('Клик по карточке открывает модал', async ({ productListPage }) => {
    await productListPage.goto();
    const name = `Просмотр_${Date.now()}`;
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name, calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await expect(productListPage.productForm).not.toBeVisible();
    await productListPage.productCards.filter({ hasText: name }).first().click();
    await expect(productListPage.page.locator('.modal-content')).toBeVisible();
  });
});