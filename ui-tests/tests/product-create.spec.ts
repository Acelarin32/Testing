import { test, expect } from '../fixtures';

test.describe('Создание продукта', () => {

  test.beforeEach(async ({ productListPage }) => {
    await productListPage.goto();
    await productListPage.openCreateForm();
  });

  test('Валидный продукт', async ({ productListPage }) => {
    const name = `Помидор_${Date.now()}`;
    await productListPage.fillProductForm({ name, calories: 100, proteins: 10, fats: 5, carbs: 20, category: '2', readiness: '0' });
    await productListPage.submitForm();
    await expect(productListPage.productForm).not.toBeVisible();
    await expect(productListPage.productCards.filter({ hasText: name }).first()).toBeVisible();
  });

  test('Без имени', async ({ productListPage }) => {
    await productListPage.fillProductForm({ name: '', calories: 100, proteins: 10, fats: 5, carbs: 20 });
    await productListPage.submitForm();
    await expect(productListPage.productForm).toBeVisible();
  });

  test('Пустое имя', async ({ productListPage }) => {
    await productListPage.nameInput.fill('');
    await productListPage.submitForm();
    await expect(productListPage.productForm).toBeVisible();
  });

  test('Название 2 символа', async ({ productListPage }) => {
    await productListPage.fillProductForm({ name: 'Ук', calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await expect(productListPage.productForm).not.toBeVisible();
  });

  test('Название 1 символ', async ({ productListPage }) => {
    await productListPage.fillProductForm({ name: 'У', calories: 100, proteins: 10, fats: 5, carbs: 20 });
    await productListPage.submitForm();
    await expect(productListPage.productForm).toBeVisible();
  });

  test('Название 200 символов', async ({ productListPage }) => {
    await productListPage.fillProductForm({ name: 'А'.repeat(200), calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await expect(productListPage.productForm).not.toBeVisible();
  });

  test('Название 201 символ', async ({ productListPage }) => {
    await productListPage.fillProductForm({ name: 'А'.repeat(201), calories: 100, proteins: 10, fats: 5, carbs: 20 });
    await productListPage.submitForm();
    await expect(productListPage.productForm).toBeVisible();
  });

  test('БЖУ = 100', async ({ productListPage }) => {
    await productListPage.fillProductForm({ name: 'Протеин', calories: 400, proteins: 50, fats: 30, carbs: 20, category: '0', readiness: '0' });
    await productListPage.submitForm();
    await expect(productListPage.productForm).not.toBeVisible();
  });

  test('БЖУ > 100', async ({ productListPage }) => {
    await productListPage.fillProductForm({ name: 'Жирный', calories: 400, proteins: 50, fats: 31, carbs: 20 });
    await productListPage.submitForm();
    await expect(productListPage.productForm).toBeVisible();
  });

  test('Все флаги', async ({ productListPage }) => {
    await productListPage.fillProductForm({ name: 'Суперфуд', calories: 100, proteins: 10, fats: 5, carbs: 20, category: '0', readiness: '0', vegan: true, glutenFree: true, sugarFree: true });
    await productListPage.submitForm();
    await expect(productListPage.productForm).not.toBeVisible();
  });
});