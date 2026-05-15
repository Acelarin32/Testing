import pytest
from dataclasses import dataclass, field
from typing import List, Optional
from uuid import UUID, uuid4

@dataclass
class Product:
    id: UUID = field(default_factory=uuid4)
    name: str = "Тестовый продукт"
    calories: float = 0.0
    proteins: float = 0.0
    fats: float = 0.0
    carbohydrates: float = 0.0


@dataclass
class DishesProduct:
    id: UUID = field(default_factory=uuid4)
    product: Optional[Product] = None
    amount: float = 0.0


@dataclass
class Dish:
    id: UUID = field(default_factory=uuid4)
    name: str = "Тестовое блюдо"
    dishes_products: List[DishesProduct] = field(default_factory=list)


def calculate_dish(dish: Dish) -> tuple[float, float, float, float]:
    calories = proteins = fats = carbs = 0.0
    for dp in dish.dishes_products:
        if dp.product is None:
            continue
        factor = dp.amount / 100.0
        calories += dp.product.calories * factor
        proteins += dp.product.proteins * factor
        fats += dp.product.fats * factor
        carbs += dp.product.carbohydrates * factor
    return calories, proteins, fats, carbs


def make_ingredient(product: Product, amount: float) -> DishesProduct:
    return DishesProduct(product=product, amount=amount)


def make_dish(*ingredients: DishesProduct) -> Dish:
    return Dish(dishes_products=list(ingredients))


@pytest.fixture
def sample_product() -> Product:
    return Product(calories=250, proteins=10, fats=5, carbohydrates=30)


def test_empty_dish_returns_zeros():
    cals, prot, fat, carb = calculate_dish(Dish())
    assert (cals, prot, fat, carb) == (0, 0, 0, 0)


def test_single_product_100g(sample_product):
    dish = make_dish(make_ingredient(sample_product, 100))
    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == 250
    assert prot == 10
    assert fat == 5
    assert carb == 30


@pytest.mark.parametrize("amount,expected_factor", [
    (50, 0.5),
    (200, 2.0),
    (75, 0.75),
])
def test_various_amounts(sample_product, amount, expected_factor):
    dish = make_dish(make_ingredient(sample_product, amount))
    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == pytest.approx(250 * expected_factor)
    assert prot == pytest.approx(10 * expected_factor)
    assert fat == pytest.approx(5 * expected_factor)
    assert carb == pytest.approx(30 * expected_factor)


def test_multiple_products_sum():
    p1 = Product(calories=100, proteins=5, fats=2, carbohydrates=10)
    p2 = Product(calories=200, proteins=15, fats=8, carbohydrates=20)
    p3 = Product(calories=50, proteins=1, fats=0.5, carbohydrates=5)

    dish = make_dish(
        make_ingredient(p1, 100),
        make_ingredient(p2, 50),
        make_ingredient(p3, 200),
    )
    cals, prot, fat, carb = calculate_dish(dish)

    assert cals == 300
    assert prot == 14.5
    assert fat == 7.0
    assert carb == 30.0


def test_zero_amount_contributes_nothing(sample_product):
    dish = make_dish(make_ingredient(sample_product, 0))
    cals, prot, fat, carb = calculate_dish(dish)
    assert (cals, prot, fat, carb) == (0, 0, 0, 0)


def test_null_product_is_skipped(sample_product):
    null_ing = DishesProduct(product=None, amount=100)
    valid = make_ingredient(sample_product, 100)
    dish = make_dish(null_ing, valid)

    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == 250
    assert prot == 10
    assert fat == 5
    assert carb == 30


def test_amount_exactly_200g():
    p = Product(calories=100, proteins=5, fats=3, carbohydrates=10)
    dish = make_dish(make_ingredient(p, 200))
    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == 200
    assert prot == 10
    assert fat == 6
    assert carb == 20


def test_amount_01g():
    p = Product(calories=100, proteins=10, fats=5, carbohydrates=20)
    dish = make_dish(make_ingredient(p, 0.1))
    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == pytest.approx(0.1)
    assert prot == pytest.approx(0.01)
    assert fat == pytest.approx(0.005)
    assert carb == pytest.approx(0.02)


def test_amount_10000g():
    p = Product(calories=100, proteins=10, fats=5, carbohydrates=20)
    dish = make_dish(make_ingredient(p, 10_000))
    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == 10_000
    assert prot == 1_000
    assert fat == 500
    assert carb == 2_000


def test_all_zero_nutrients():
    p = Product(calories=0, proteins=0, fats=0, carbohydrates=0)
    dish = make_dish(make_ingredient(p, 500))
    cals, prot, fat, carb = calculate_dish(dish)
    assert (cals, prot, fat, carb) == (0, 0, 0, 0)


def test_max_bju_100g_each():
    p = Product(calories=900, proteins=100, fats=100, carbohydrates=100)
    dish = make_dish(make_ingredient(p, 50))
    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == 450
    assert prot == 50
    assert fat == 50
    assert carb == 50

def test_negative_calories_handled():
    p = Product(calories=-100, proteins=10, fats=5, carbohydrates=20)
    dish = make_dish(make_ingredient(p, 100))
    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == -100
    assert prot == 10
    assert fat == 5
    assert carb == 20


def test_negative_amount_handled():
    p = Product(calories=100, proteins=10, fats=5, carbohydrates=20)
    dish = make_dish(make_ingredient(p, -50))
    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == -50
    assert prot == -5
    assert fat == -2.5
    assert carb == -10


def test_all_negative_values():
    p = Product(calories=-200, proteins=-15, fats=-8, carbohydrates=-30)
    dish = make_dish(make_ingredient(p, 50))
    cals, prot, fat, carb = calculate_dish(dish)
    assert cals == -100
    assert prot == -7.5
    assert fat == -4
    assert carb == -15

def test_mixed_zero_fractional_normal():
    p1 = Product(calories=200, proteins=10, fats=8, carbohydrates=25)
    p2 = Product(calories=150, proteins=5, fats=3, carbohydrates=15)
    p3 = Product(calories=300, proteins=20, fats=10, carbohydrates=30)

    dish = make_dish(
        make_ingredient(p1, 75),
        make_ingredient(p2, 0),
        make_ingredient(p3, 33.3),
    )
    cals, prot, fat, carb = calculate_dish(dish)

    assert cals == pytest.approx(249.9, rel=0.01)
    assert prot == pytest.approx(14.16, rel=0.01)
    assert fat == pytest.approx(9.33, rel=0.01)
    assert carb == pytest.approx(28.74, rel=0.01)

