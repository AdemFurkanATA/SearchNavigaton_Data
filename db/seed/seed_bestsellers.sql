INSERT INTO bestsellers_by_category (category, product_id, buyer_count, rank, updated_at)
VALUES
('electronics', 'product-1', 3, 1, NOW()),
('electronics', 'product-8', 1, 2, NOW()),
('electronics', 'product-2', 1, 3, NOW()),
('clothing', 'product-3', 1, 1, NOW()),
('clothing', 'product-4', 0, 2, NOW()),
('home', 'product-5', 1, 1, NOW()),
('home', 'product-6', 0, 2, NOW()),
('accessories', 'product-9', 0, 1, NOW()),
('accessories', 'product-10', 0, 2, NOW()),
('books', 'product-7', 0, 1, NOW());

INSERT INTO bestsellers_general (product_id, buyer_count, rank, updated_at)
VALUES
('product-1', 3, 1, NOW()),
('product-5', 1, 2, NOW()),
('product-3', 1, 3, NOW()),
('product-8', 1, 4, NOW()),
('product-2', 1, 5, NOW()),
('product-9', 0, 6, NOW()),
('product-10', 0, 7, NOW()),
('product-7', 0, 8, NOW()),
('product-6', 0, 9, NOW()),
('product-4', 0, 10, NOW());
