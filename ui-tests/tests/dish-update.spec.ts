import { test, expect } from '../fixtures';

test.describe('Редактирование блюда', () => {
  test.beforeEach(async ({ productListPage }) => {
    await productListPage.goto();
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name: 'ДляРедакта', calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
  });

  test('Изменить название', async ({ dishListPage }) => {
    await dishListPage.goto();
    const oldName = `ДоБлюдо_${Date.now()}`;
    await dishListPage.openCreateForm();
    await dishListPage.fillDishForm({ name: oldName, portionSize: 200, category: '2', ingredients: [{ productName: 'ДляРедакта', amount: 100 }] });
    await dishListPage.submitForm();
    await dishListPage.dishCards.filter({ hasText: oldName }).first().click();
    await expect(dishListPage.page.locator('.modal-content')).toBeVisible();
    const newName = `ПослеБлюдо_${Date.now()}`;
    await dishListPage.page.locator('.modal-content input[type="text"]').first().fill(newName);
    await dishListPage.page.locator('.modal-content button[type="submit"]').click();
    await expect(dishListPage.page.locator('.modal-content')).not.toBeVisible();
    await expect(dishListPage.dishCards.filter({ hasText: newName }).first()).toBeVisible();
  });

  test('Закрыть модал без сохранения', async ({ dishListPage }) => {
    await dishListPage.goto();
    const name = `ЗакрытьБлюдо_${Date.now()}`;
    await dishListPage.openCreateForm();
    await dishListPage.fillDishForm({ name, portionSize: 200, category: '2', ingredients: [{ productName: 'ДляРедакта', amount: 100 }] });
    await dishListPage.submitForm();
    await dishListPage.dishCards.filter({ hasText: name }).first().click();
    await dishListPage.page.locator('.modal-close').click();
    await expect(dishListPage.page.locator('.modal-content')).not.toBeVisible();
  });
});