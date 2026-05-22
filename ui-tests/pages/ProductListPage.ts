import { Page, Locator } from '@playwright/test';

export class ProductListPage {
  readonly page: Page;
  readonly addButton: Locator;
  readonly searchInput: Locator;
  readonly categorySelect: Locator;
  readonly readinessSelect: Locator;
  readonly veganCheckbox: Locator;
  readonly glutenFreeCheckbox: Locator;
  readonly sugarFreeCheckbox: Locator;
  readonly sortBySelect: Locator;
  readonly sortOrderButton: Locator;
  readonly productCards: Locator;
  readonly productForm: Locator;
  readonly nameInput: Locator;
  readonly caloriesInput: Locator;
  readonly proteinsInput: Locator;
  readonly fatsInput: Locator;
  readonly carbsInput: Locator;
  readonly formCategorySelect: Locator;
  readonly formReadinessSelect: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly formErrors: Locator;

  constructor(page: Page) {
    this.page = page;
    this.addButton = page.locator('.btn-add');
    this.searchInput = page.locator('.control-block--search input');
    this.categorySelect = page.locator('.control-block select').first();
    this.readinessSelect = page.locator('.control-block select').nth(1);
    this.veganCheckbox = page.locator('.control-block__checkboxes label').filter({ hasText: 'Веган' }).locator('input');
    this.glutenFreeCheckbox = page.locator('.control-block__checkboxes label').filter({ hasText: 'Без глютена' }).locator('input');
    this.sugarFreeCheckbox = page.locator('.control-block__checkboxes label').filter({ hasText: 'Без сахара' }).locator('input');
    this.sortBySelect = page.locator('.control-block__row select');
    this.sortOrderButton = page.locator('.control-block__row button');
    this.productCards = page.locator('.product-card');
    this.productForm = page.locator('.product-form');
    this.nameInput = page.locator('.product-form input[type="text"]').first();
    this.caloriesInput = page.locator('.product-form input[type="number"]').nth(0);
    this.proteinsInput = page.locator('.product-form input[type="number"]').nth(1);
    this.fatsInput = page.locator('.product-form input[type="number"]').nth(2);
    this.carbsInput = page.locator('.product-form input[type="number"]').nth(3);
    this.formCategorySelect = page.locator('.product-form select').first();
    this.formReadinessSelect = page.locator('.product-form select').nth(1);
    this.submitButton = page.locator('.product-form button[type="submit"]');
    this.cancelButton = page.locator('.product-form button[type="button"]');
    this.formErrors = page.locator('.form-errors');
  }

  async goto() {
    await this.page.goto('/products');
  }

  async openCreateForm() {
    await this.addButton.click();
    await this.productForm.waitFor({ state: 'visible' });
  }

  async fillProductForm(data: {
    name: string;
    calories?: number;
    proteins?: number;
    fats?: number;
    carbs?: number;
    composition?: string;
    category?: string;
    readiness?: string;
    vegan?: boolean;
    glutenFree?: boolean;
    sugarFree?: boolean;
  }) {
    await this.nameInput.fill(data.name);
    if (data.calories !== undefined) await this.caloriesInput.fill(String(data.calories));
    if (data.proteins !== undefined) await this.proteinsInput.fill(String(data.proteins));
    if (data.fats !== undefined) await this.fatsInput.fill(String(data.fats));
    if (data.carbs !== undefined) await this.carbsInput.fill(String(data.carbs));
    if (data.category) await this.formCategorySelect.selectOption(data.category);
    if (data.readiness) await this.formReadinessSelect.selectOption(data.readiness);
  }

  async submitForm() {
    await this.submitButton.click();
  }

  async searchProduct(name: string) {
    await this.searchInput.fill(name);
  }

  async selectCategory(value: string) {
    await this.categorySelect.selectOption(value);
  }

  async selectReadiness(value: string) {
    await this.readinessSelect.selectOption(value);
  }

  async toggleVegan(checked: boolean) {
    if (checked) await this.veganCheckbox.check();
    else await this.veganCheckbox.uncheck();
  }

  async sortBy(value: string) {
    await this.sortBySelect.selectOption(value);
  }

async toggleSortOrder() {
    await this.sortOrderButton.dispatchEvent('click');
  }

  async getProductCount(): Promise<number> {
    return await this.productCards.count();
  }

  async deleteProductByName(name: string) {
    const card = this.productCards.filter({ hasText: name }).first();
    this.page.on('dialog', async dialog => await dialog.accept());
    await card.locator('.product-card-delete').click();
  }
}