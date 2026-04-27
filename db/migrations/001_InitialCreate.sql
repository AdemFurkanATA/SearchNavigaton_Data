CREATE TABLE IF NOT EXISTS bestsellers_by_category (
    id SERIAL PRIMARY KEY,
    category VARCHAR NOT NULL,
    product_id VARCHAR NOT NULL,
    buyer_count INT NOT NULL,
    rank INT NOT NULL,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_bestsellers_category
ON bestsellers_by_category(category);

CREATE TABLE IF NOT EXISTS bestsellers_general (
    id SERIAL PRIMARY KEY,
    product_id VARCHAR NOT NULL,
    buyer_count INT NOT NULL,
    rank INT NOT NULL,
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);
