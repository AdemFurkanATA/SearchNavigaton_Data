INSERT INTO product (id, name, category, price)
VALUES
('product-1', 'Wireless Headphones', 'electronics', 79.99),
('product-2', 'Gaming Mouse', 'electronics', 39.99),
('product-3', 'Cotton T-Shirt', 'clothing', 14.99),
('product-4', 'Running Shoes', 'clothing', 64.99),
('product-5', 'Coffee Maker', 'home', 49.99),
('product-6', 'Desk Lamp', 'home', 19.99),
('product-7', 'Notebook', 'books', 9.99),
('product-8', 'Smart Watch', 'electronics', 129.99),
('product-9', 'Backpack', 'accessories', 34.99),
('product-10', 'Water Bottle', 'accessories', 11.99)
ON CONFLICT (id) DO NOTHING;
