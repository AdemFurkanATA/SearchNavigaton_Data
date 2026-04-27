CREATE INDEX IF NOT EXISTS idx_bestsellers_general_rank
ON bestsellers_general(rank);

CREATE INDEX IF NOT EXISTS idx_bestsellers_by_category_rank
ON bestsellers_by_category(category, rank);
