import { Page, Locator } from '@playwright/test';

export class DishListPage {
  readonly page: Page;
  readonly addButton: Locator;
  readonly searchInput: Locator;
  readonly categorySelect: Locator;
  readonly veganCheckbox: Locator;
  readonly glutenFreeCheckbox: Locator;
  readonly sugarFreeCheckbox: Locator;
  readonly sortBySelect: Locator;
  readonly sortOrderButton: Locator;
  readonly dishCards: Locator;
  readonly dishForm: Locator;
  readonly nameInput: Locator;
  readonly portionSizeInput: Locator;
  readonly formCategorySelect: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly ingredientSelect: Locator;
  readonly ingredientAmount: Locator;
  readonly addIngredientButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.addButton = page.locator('.btn-add');
    this.searchInput = page.locator('.control-block--search input');
    this.categorySelect = page.locator('.control-block select').first();
    this.veganCheckbox = page.locator('.control-block__checkboxes label').filter({ hasText: 'Веган' }).locator('input');
    this.glutenFreeCheckbox = page.locator('.control-block__checkboxes label').filter({ hasText: 'Без глютена' }).locator('input');
    this.sugarFreeCheckbox = page.locator('.control-block__checkboxes label').filter({ hasText: 'Без сахара' }).locator('input');
    this.sortBySelect = page.locator('.control-block__row select');
    this.sortOrderButton = page.locator('.control-block__row button');
    this.dishCards = page.locator('.dish-card');
    this.dishForm = page.locator('.dish-form');
    this.nameInput = page.locator('.dish-form input[type="text"]').first();
    this.portionSizeInput = page.locator('.dish-form input[type="number"]').first();
    this.formCategorySelect = page.locator('.dish-form select').first();
    this.submitButton = page.locator('.dish-form button[type="submit"]');
    this.cancelButton = page.locator('.dish-form button[type="button"]').first();
    this.ingredientSelect = page.locator('.ingredient-row select');
    this.ingredientAmount = page.locator('.ingredient-row input[type="number"]');
    this.addIngredientButton = page.locator('.btn-add-ingredient');
  }

  async goto() {
    await this.page.goto('/dishes');
  }

  async openCreateForm() {
    await this.addButton.click();
    await this.dishForm.waitFor({ state: 'visible' });
  }

  async fillDishForm(data: {
    name: string;
    portionSize: number;
    category?: string;
    ingredients: { productName: string; amount: number }[];
  }) {
    await this.nameInput.fill(data.name);
    await this.portionSizeInput.fill(String(data.portionSize));
    if (data.category) await this.formCategorySelect.selectOption(data.category);

    for (let i = 0; i < data.ingredients.length; i++) {
      if (i > 0) await this.addIngredientButton.click();
      const select = this.ingredientSelect.nth(i);
      const amount = this.ingredientAmount.nth(i);

      const option = select.locator('option').filter({ hasText: data.ingredients[i].productName }).first();
      const value = await option.getAttribute('value');
      if (value) {
        await select.selectOption(value);
      } else {
        await select.selectOption({ index: 1 });
      }
      await amount.fill(String(data.ingredients[i].amount));
    }
  }

  async submitForm() {
    await this.submitButton.click();
  }

  async searchDish(name: string) {
    await this.searchInput.fill(name);
  }

  async selectCategory(value: string) {
    await this.categorySelect.selectOption(value);
  }

  async sortBy(value: string) {
    await this.sortBySelect.selectOption(value);
  }

  async getDishCount(): Promise<number> {
    return await this.dishCards.count();
  }

  async deleteDishByName(name: string) {
      const card = this.dishCards.filter({ hasText: name }).first();
      this.page.once('dialog', async dialog => await dialog.accept());
      await card.locator('.dish-card__delete').click();
  }

  async toggleSortOrder() {
    await this.sortOrderButton.dispatchEvent('click');
  }
}