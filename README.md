# E-Commerce Recommendation System

A real-time product recommendation system for e-commerce platforms, built with .NET 8, Kafka, Redis, and PostgreSQL in a microservices architecture.

## 🎯 Features

- **Browsing History**: Track and retrieve user's last 10 viewed products
- **Personalized Best Sellers**: Recommend products based on user's browsing history categories
- **Non-Personalized Best Sellers**: Fall back to general best sellers for new users
- **Real-time Processing**: Kafka-based event streaming with sub-second latency
- **Business Rules**: Minimum 5 products, max 3 categories, distinct user count metric

## 🏗️ Architecture

```
┌─────────────────┐     ┌──────────────┐     ┌─────────────────┐
│ product-views   │────▶│ ViewProducer │────▶│ Apache Kafka    │
│ .json           │     │ Worker       │     │ product-views   │
└─────────────────┘     └──────────────┘     └────────┬────────┘
                                                       │
                                                       ▼
┌─────────────────┐     ┌──────────────┐     ┌─────────────────┐
│ PostgreSQL      │◀────│ ETL Service  │     │ ViewStream      │
│ (orders)        │     │ (Quartz.NET) │     │ Worker          │
└────────┬────────┘     └──────────────┘     └────────┬────────┘
         │                                            │
         │                                            ▼
         │                                     ┌─────────────────┐
         │                                     │ Redis           │
         │                                     │ Sorted Set      │
         └───────────────┬───────────────────────────┘
                        │
                        ▼
                 ┌─────────────────┐
                 │ Recommendation  │
                 │ API             │
                 └────────┬────────┘
                          │
                          ▼
                 ┌─────────────────┐
                 │ Web/Mobile      │
                 │ Client          │
                 └─────────────────┘
```

## 📁 Project Structure

```
EcommerceRecommendation/
├── src/
│   ├── ViewProducer.Worker/      # Kafka producer (1 event/sec)
│   ├── ViewStream.Worker/        # Kafka consumer + Redis writer
│   ├── Recommendation.ETL/       # Batch job for bestseller calculation
│   ├── Recommendation.API/       # REST API
│   ├── Recommendation.Data/      # EF Core DbContext
│   └── Recommendation.Shared/    # Common models and constants
├── tests/
│   ├── Recommendation.API.Tests/
│   └── Recommendation.ETL.Tests/
├── db/
│   ├── migrations/
│   └── seed/
├── docker-compose.yml
└── EcommerceRecommendation.sln
```

## 🚀 Quick Start

### Prerequisites
- Docker Desktop
- .NET 8 SDK (for local development)

### Run with Docker

```bash
# Start all services
docker compose up -d

# Check status
docker compose ps

# View logs
docker compose logs -f api
```

### Local Development

```bash
# Restore dependencies
dotnet restore EcommerceRecommendation.sln

# Build
dotnet build EcommerceRecommendation.sln

# Run tests
dotnet test EcommerceRecommendation.sln

# Run API locally
dotnet run --project src/Recommendation.API/Recommendation.API.csproj
```

## 📡 API Endpoints

### Get Browsing History
```http
GET /browsing-history/{userId}
```

**Response (200 OK):**
```json
{
  "user-id": "user-120",
  "products": ["product-8", "product-7", "product-6", "product-5", "product-4"],
  "type": "personalized"
}
```

### Delete Product from History
```http
DELETE /browsing-history/{userId}/{productId}
```

**Response (204 No Content):**
```
(no content)
```

### Get Best Sellers
```http
GET /best-sellers/{userId}
```

**Response - Personalized (200 OK):**
```json
{
  "user-id": "user-120",
  "products": ["product-1", "product-8", "product-2", "product-3", "product-4"],
  "type": "personalized"
}
```

**Response - Non-Personalized (200 OK):**
```json
{
  "user-id": "user-999",
  "products": ["product-1", "product-5", "product-3", "product-8", "product-2"],
  "type": "non-personalized"
}
```

### Search Products
```http
GET /search?q={query}&strategy={prefix|fuzzy|tfidf}&limit={n}
```

**Response (200 OK):**
```json
{
  "query": "head",
  "strategy": "prefix",
  "results": [
    { "productId": "product-1", "name": "Wireless Headphones", "category": "electronics", "score": 1.0 }
  ],
  "elapsed-ms": 3
}
```

## 🧪 Testing

```bash
# Run all tests
dotnet test EcommerceRecommendation.sln

# Run with coverage
dotnet test EcommerceRecommendation.sln --collect:"XPlat Code Coverage"
```

**Test Coverage:**
- Controller tests (BrowsingHistory, BestSellers)
- Service tests (BrowsingHistory, BestSeller)
- ETL job tests
- Search module tests (Trie, Fuzzy, TF-IDF, Cache, SearchController)

## 🔍 Search Module

- **Strategies:** Prefix (Trie), Fuzzy (Levenshtein), TF-IDF
- **Thread-safety:** Immutable Trie for reads, ConcurrentDictionary for TF-IDF and cache
- **Cache:** TTL-based (configurable), key = `{strategy}:{query}:{limit}`
- **Defaults:** strategy=`prefix`, limit=`10`, cache TTL=`5 min`

## 📈 Benchmarks

Run benchmarks:
```bash
dotnet run --project benchmarks/Recommendation.Benchmarks -c Release
```

Latest summary (example):
- PrefixSearch: ~290 ns (100–10,000 products)
- FuzzySearch: ~301–311 ns
- TfIdfSearch: ~301–307 ns
- ListAccess: ~25.7 µs
- ConcurrentDictionaryAccess: ~285 µs

## 🔧 Configuration

### Environment Variables

| Service | Variable | Default |
|---------|----------|---------|
| All | Kafka:BootstrapServers | kafka:29092 |
| All | Redis:ConnectionString | redis:6379 |
| All | ConnectionStrings:DefaultConnection | Host=postgres;... |
| ETL | Quartz:BestSellersCronSchedule | 0 0 2 * * ? |

### Docker Compose Services

```yaml
services:
  zookeeper       # Kafka coordination
  kafka           # Message broker (port 9092)
  postgres        # Database (port 5432)
  redis           # Cache (port 6379)
  view-producer   # Kafka producer
  view-stream     # Kafka consumer
  etl             # Batch job
  api             # REST API (port 8080)
```

## 📄 Database Schema

### Tables (Provided)
- `product` - Product catalog (id, name, category, price)
- `order` - Order headers (id, user_id, created_at)
- `order_item` - Order lines (id, order_id, product_id, quantity)

### Tables (Created by ETL)
- `bestsellers_by_category` - Category-based best sellers
- `bestsellers_general` - General best sellers

## 📊 Database Context (EF Core)

The `ApplicationDbContext` class maps the above tables to entities:
- `DbSet<Product> Products`
- `DbSet<Order> Orders`
- `DbSet<OrderItem> OrderItems`
- `DbSet<BestSellerByCategory> BestsellersByCategory`
- `DbSet<BestSellerGeneral> BestsellersGeneral`

Entity configurations (primary keys, column names, relationships) are defined in `OnModelCreating` to match the schema exactly.

## 🔍 Monitoring

```bash
# Check container logs
docker compose logs -f api
docker compose logs -f view-producer
docker compose logs -f view-stream

# Check Redis data
docker compose exec redis redis-cli KEYS "*"
docker compose exec redis redis-cli ZREVRANGE browsing_history:user-120 0 -1

# Check PostgreSQL data
docker compose exec postgres psql -U postgres -d data-db -c "SELECT * FROM bestsellers_by_category;"
```

## 🚨 Troubleshooting

### Kafka Connection Issues
```bash
# Check Kafka is running
docker compose ps kafka

# Check Kafka logs
docker compose logs kafka

# List Kafka topics
docker compose exec kafka kafka-topics --bootstrap-server localhost:9092 --list
```

### Database Issues
```bash
# Check tables exist
docker compose exec postgres psql -U postgres -d data-db -c "\dt"

# Run migrations
docker compose cp db/migrations/001_InitialCreate.sql postgres:/tmp/
docker compose exec postgres psql -U postgres -d data-db -f /tmp/001_InitialCreate.sql
```

## 📄 License

This project is for educational purposes.

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Run tests: `dotnet test`
4. Submit a pull request

---

**Built with ❤️ using .NET 8, Kafka, Redis, and PostgreSQL**