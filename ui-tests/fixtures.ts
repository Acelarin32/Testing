import { test as base } from '@playwright/test';
import { ProductListPage } from './pages/ProductListPage.ts';
import { DishListPage } from './pages/DishListPage.ts';

type MyFixtures = {
  productListPage: ProductListPage;
  dishListPage: DishListPage;
};

export const test = base.extend<MyFixtures>({
  productListPage: async ({ page }, use) => {
    await use(new ProductListPage(page));
  },
  dishListPage: async ({ page }, use) => {
    await use(new DishListPage(page));
  },
});

export { expect } from '@playwright/test';