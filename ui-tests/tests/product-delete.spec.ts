import { test, expect } from '../fixtures';

test.describe('Удаление продукта', () => {
  test('Удалить свободный продукт', async ({ productListPage }) => {
    await productListPage.goto();
    const name = `Удалить_${Date.now()}`;
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name, calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await expect(productListPage.productForm).not.toBeVisible();
    await productListPage.page.waitForTimeout(500);
    const before = await productListPage.getProductCount();
    await productListPage.deleteProductByName(name);
    await productListPage.page.waitForTimeout(1000);
    const after = await productListPage.getProductCount();
    expect(after).toBe(before - 1);
  });

  test('Кнопка удаления видна на карточке', async ({ productListPage }) => {
    await productListPage.goto();
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name: 'КнопкаУдаления', calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await expect(productListPage.productCards.first().locator('.product-card-delete')).toBeVisible();
  });
});