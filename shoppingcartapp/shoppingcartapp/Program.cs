using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShoppingCartApp
{
    // Category class represents a product category.
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Category(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    // Product class now includes a reference to the category it belongs to.
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public Category Category { get; set; }

        public Product(int id, string name, decimal price, string description, Category category)
        {
            Id = id;
            Name = name;
            Price = price;
            Description = description;
            Category = category;
        }
    }

    // CartItem class represents an item added to the shopping cart, including the quantity.
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }

        public CartItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public decimal TotalPrice()
        {
            return Product.Price * Quantity;
        }
    }

    // Cart class manages the shopping cart.
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();
        private const int cartExpirationInMinutes = 30;
        private DateTime expirationTime;

        public Cart()
        {
            expirationTime = DateTime.Now.AddMinutes(cartExpirationInMinutes);
        }

        public void AddItem(Product product, int quantity)
        {
            var existingItem = items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                items.Add(new CartItem(product, quantity));
            }
        }

        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.Product.Id == productId);
        }

        public void ViewCart()
        {
            if (items.Count == 0)
            {
                Console.WriteLine("Your cart is empty.");
                return;
            }

            Console.WriteLine("Items in your cart:");
            foreach (var item in items)
            {
                Console.WriteLine($"{item.Product.Name} - {item.Quantity} x ${item.Product.Price} = ${item.TotalPrice()}");
            }
        }

        public decimal CalculateTotal()
        {
            return items.Sum(i => i.TotalPrice());
        }

        public bool IsCartExpired()
        {
            return DateTime.Now > expirationTime;
        }

        public List<Product> RecommendProducts(List<Product> allProducts)
        {
            return allProducts.Where(p => !items.Any(i => i.Product.Id == p.Id)).Take(3).ToList();
        }
    }

    public class Checkout
    {
        public decimal Discount { get; set; }
        public decimal TaxRate { get; set; }

        public Checkout(decimal discount, decimal taxRate)
        {
            Discount = discount;
            TaxRate = taxRate;
        }

        public decimal ApplyDiscount(decimal total)
        {
            return total - (total * Discount / 100);
        }

        public decimal ApplyTax(decimal total)
        {
            return total + (total * TaxRate / 100);
        }

        public decimal FinalizeTotal(decimal total)
        {
            total = ApplyDiscount(total);
            total = ApplyTax(total);
            return total;
        }
    }

    public class Program
    {
        static List<Category> categories = new List<Category>();
        static List<Product> products = new List<Product>();

        static void Main(string[] args)
        {
            LoadCategoriesFromFile(@"D:/inventory/category.txt");
            LoadProductsFromFile(@"D:/inventory/inventory.txt");

            Cart cart = new Cart();
            Checkout checkout = new Checkout(discount: 10, taxRate: 5);

            bool shopping = true;
            while (shopping)
            {
                Console.WriteLine("\n--- Shopping Cart Menu ---");
                Console.WriteLine("1. View Products by Category");
                Console.WriteLine("2. Add Product to Cart");
                Console.WriteLine("3. View Cart");
                Console.WriteLine("4. Remove Product from Cart");
                Console.WriteLine("5. Checkout");
                Console.WriteLine("6. View Product Recommendations");
                Console.WriteLine("7. Exit");
                Console.Write("Choose an option: ");
                int choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        ViewProductsByCategory();
                        break;

                    case 2:
                        AddProductToCart(cart);
                        break;

                    case 3:
                        cart.ViewCart();
                        break;

                    case 4:
                        RemoveProductFromCart(cart);
                        break;

                    case 5:
                        CheckoutCart(cart, checkout);
                        shopping = false;
                        break;

                    case 6:
                        ViewRecommendations(cart);
                        break;

                    case 7:
                        shopping = false;
                        Console.WriteLine("Thank you for shopping!");
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        static void ViewProductsByCategory()
        {
            Console.WriteLine("\nAvailable Categories:");
            foreach (var category in categories)
            {
                Console.WriteLine($"{category.Id}. {category.Name}");
            }

            Console.Write("\nEnter Category ID to view products: ");
            int categoryId = int.Parse(Console.ReadLine());
            var selectedCategory = categories.FirstOrDefault(c => c.Id == categoryId);

            if (selectedCategory != null)
            {
                var productsInCategory = products.Where(p => p.Category.Id == selectedCategory.Id).ToList();
                Console.WriteLine($"\nProducts in {selectedCategory.Name}:");
                foreach (var product in productsInCategory)
                {
                    Console.WriteLine($"{product.Id}. {product.Name} - ${product.Price} ({product.Description})");
                }
            }
            else
            {
                Console.WriteLine("Invalid category ID.");
            }
        }

        static void AddProductToCart(Cart cart)
        {
            Console.Write("\nEnter Product ID to add: ");
            int productId = int.Parse(Console.ReadLine());
            Console.Write("Enter Quantity: ");
            int quantity = int.Parse(Console.ReadLine());

            var productToAdd = products.FirstOrDefault(p => p.Id == productId);
            if (productToAdd != null)
            {
                cart.AddItem(productToAdd, quantity);
                Console.WriteLine($"{quantity} {productToAdd.Name}(s) added to your cart.");
            }
            else
            {
                Console.WriteLine("Invalid product ID.");
            }
        }

        static void RemoveProductFromCart(Cart cart)
        {
            Console.Write("\nEnter Product ID to remove: ");
            int removeProductId = int.Parse(Console.ReadLine());
            cart.RemoveItem(removeProductId);
            Console.WriteLine("Item removed from your cart.");
        }

        static void CheckoutCart(Cart cart, Checkout checkout)
        {
            if (cart.IsCartExpired())
            {
                Console.WriteLine("Your cart has expired.");
            }
            else
            {
                decimal subtotal = cart.CalculateTotal();
                Console.WriteLine($"\nSubtotal: ${subtotal}");
                decimal finalTotal = checkout.FinalizeTotal(subtotal);
                Console.WriteLine($"Total after discount and tax: ${finalTotal}");
            }
        }

        static void ViewRecommendations(Cart cart)
        {
            var recommendations = cart.RecommendProducts(products);
            Console.WriteLine("\nProduct Recommendations:");
            foreach (var recommendation in recommendations)
            {
                Console.WriteLine($"{recommendation.Name} - ${recommendation.Price}");
            }
        }

        // Load categories from categories.txt
        static void LoadCategoriesFromFile(string fileName)
        {
            try
            {
                using (var reader = new StreamReader(fileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        if (parts.Length == 2)
                        {
                            int categoryId = int.Parse(parts[0]);
                            string categoryName = parts[1];
                            categories.Add(new Category(categoryId, categoryName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading categories file: {ex.Message}");
            }
        }

        // Load products from products.txt
        static void LoadProductsFromFile(string fileName)
        {
            try
            {
                using (var reader = new StreamReader(fileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        if (parts.Length == 5)
                        {
                            int productId = int.Parse(parts[0]);
                            string productName = parts[1];
                            decimal productPrice = decimal.Parse(parts[2]);
                            string productDescription = parts[3];
                            int categoryId = int.Parse(parts[4]);

                            var category = categories.FirstOrDefault(c => c.Id == categoryId);
                            if (category != null)
                            {
                                products.Add(new Product(productId, productName, productPrice, productDescription, category));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading products file: {ex.Message}");
            }
        }
    }
}
