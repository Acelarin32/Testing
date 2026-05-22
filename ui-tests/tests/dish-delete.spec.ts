import { test, expect } from '../fixtures';

test.describe('Удаление блюда', () => {
  test.beforeEach(async ({ productListPage }) => {
    await productListPage.goto();
    await productListPage.openCreateForm();
    await productListPage.fillProductForm({ name: 'ДляУдаленияБлюда', calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await expect(productListPage.productForm).not.toBeVisible();
  });

  test('Удалить блюдо', async ({ dishListPage }) => {
    await dishListPage.goto();
    const name = `УдалитьБлюдо_${Date.now()}`;
    await dishListPage.openCreateForm();
    await dishListPage.fillDishForm({ name, portionSize: 100, category: '2', ingredients: [{ productName: 'ДляУдаленияБлюда', amount: 50 }] });
    await dishListPage.submitForm();
    await expect(dishListPage.dishForm).not.toBeVisible();
    await dishListPage.page.waitForTimeout(500);
    const before = await dishListPage.getDishCount();
    await dishListPage.deleteDishByName(name);
    await dishListPage.page.waitForTimeout(1000);
    const after = await dishListPage.getDishCount();
    expect(after).toBeLessThan(before);
  });
});