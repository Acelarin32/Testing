import { test, expect } from '../fixtures';

test.describe('Редактирование продукта', () => {
  test('Изменить название', async ({ productListPage }) => {
    await productListPage.goto();
    const oldName = `До_${Date.now()}`;
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name: oldName, calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await productListPage.productCards.filter({ hasText: oldName }).first().click();
    await expect(productListPage.page.locator('.modal-content')).toBeVisible();
    const newName = `После_${Date.now()}`;
    await productListPage.page.locator('.modal-content input[type="text"]').first().fill(newName);
    await productListPage.page.locator('.modal-content button[type="submit"]').click();
    await expect(productListPage.page.locator('.modal-content')).not.toBeVisible();
    await expect(productListPage.productCards.filter({ hasText: newName }).first()).toBeVisible();
  });

  test('Закрыть модал без сохранения', async ({ productListPage }) => {
    await productListPage.goto();
    const name = `Закрыть_${Date.now()}`;
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name, calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await productListPage.productCards.filter({ hasText: name }).first().click();
    await productListPage.page.locator('.modal-close').click();
    await expect(productListPage.page.locator('.modal-content')).not.toBeVisible();
  });
});