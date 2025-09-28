# 🧩 Microservices Demo (.NET + SQLite)

This project is a **learning-oriented microservices architecture** built with **ASP.NET Core Web API** and **Entity Framework Core (SQLite)**.  

The goal is to understand the basics of microservices step by step:  
- Independent services  
- Database per service  
- Communication between services  
- Resilience & containerization (later with Docker)

---

## 📚 What are Microservices?

- **Microservice** = a small, independent application that does *one thing well*.  
- Each service:  
  - Has its own codebase  
  - Runs independently  
  - Owns its own database  
  - Talks to others via APIs  

### Monolith vs Microservices

| Monolith 🏛 | Microservices 🧩 |
|-------------|-----------------|
| Single codebase, tightly coupled | Many small, independent services |
| One shared database | One database per service |
| Easy to start, harder to scale | More complex, but scalable & resilient |

---

## 🏗️ Services in this Project

We simulate a **mini e-commerce system** with 3 services:

- **User Service** → manages users (`users.db`)
- **Product Service** → manages products (`products.db`)
- **Order Service** → manages orders (`orders.db`), validates users and products

---

## 📂 Solution Structure

```
MicroservicesDemo.sln
├── UserService/          # Manages users
├── ProductService/       # Manages products
└── OrderService/         # Manages orders
```

Each service is:
- An **ASP.NET Core Web API**
- Uses **EF Core + SQLite**
- Has its own **database file**
- Runs on its own port

---

## ⚙️ Running the Services

Open a terminal for each service and run:

```bash
# User Service (http://localhost:5001)
cd UserService
dotnet run

# Product Service (http://localhost:5003)
cd ProductService
dotnet run

# Order Service (http://localhost:5005)
cd OrderService
dotnet run
```

### 🐳 Running with Docker Compose

To start all services together with persistent SQLite databases, use Docker Compose from the project root:

```bash
docker compose up --build
```

Database files are stored on the host inside the `data/` directory (one subfolder per service), so they survive container restarts.

> **RabbitMQ Dashboard** — Once Compose is running, open http://localhost:15672 (default user/password: `guest`/`guest`) to inspect queues, messages, and consumers that MassTransit creates.

### 🐇 RabbitMQ & MassTransit Setup

- Docker Compose spins up a dedicated `rabbitmq:3-management` container alongside the APIs.
- `OrderService` publishes an `OrderCreatedEvent` (via MassTransit) whenever an order is persisted.
- `ProductService` hosts a MassTransit consumer that listens for `OrderCreatedEvent` and decrements product stock.
- Connection settings can be overridden via `RabbitMq__*` environment variables (see `appsettings.Development.json` for local defaults).

---

## 🔎 Endpoints

### 👤 User Service
- `GET /api/users` → list users  
- `GET /api/users/{id}` → get user by ID  
- `POST /api/users` → create a user  

Example request:
```json
{ "name": "Alice", "email": "alice@example.com" }
```

---

### 📦 Product Service
- `GET /api/products` → list products  
- `GET /api/products/{id}` → get product by ID  
- `POST /api/products` → create a product  

Example request:
```json
{ "name": "Laptop", "price": 1200.00, "stock": 5 }
```

---

### 📝 Order Service
- `GET /api/orders/{id}` → get order by ID  
- `POST /api/orders` → create a new order  

Example request:
```json
{ "userId": "GUID", "productId": "GUID", "quantity": 2 }
```

ℹ️ Orders are created synchronously over HTTP. After persistence the service publishes an `OrderCreatedEvent`, which triggers Product Service to adjust inventory.

---

## 📊 Architecture (Step 1)

```mermaid
graph TD
  U[User Service] -->|manages users| DB1[(users.db)]
  P[Product Service] -->|manages products| DB2[(products.db)]
  O[Order Service] -->|manages orders| DB3[(orders.db)]
```

---

## 📌 Step 2: Service Communication (REST)

The **Order Service** now orchestrates synchronous validations via **REST APIs**:

- ✅ Checks the user exists in User Service
- ✅ Fetches product details & stock from Product Service
- ✅ Uses Polly policies (retry & circuit breaker) for resilience when calling external APIs

## 🚀 Step 4: Event-Driven Choreography

To remove tight coupling after an order is placed we introduced RabbitMQ + MassTransit:

1. Order API saves the order and publishes an `OrderCreatedEvent`.
2. Product Service consumes the event and decrements inventory.
3. RabbitMQ keeps a durable queue so no order events are lost if Product Service is offline temporarily.
4. You can watch the message flow via the management UI at http://localhost:15672.

### Future Topics
- API Gateway (single entry point)
- Additional asynchronous workflows (refunds, restocking, etc.)
- Docker & Docker Compose optimisations
- Deployment to cloud

---

## 🛠️ Development Notes

- **.gitignore** excludes build artifacts, local DBs, and IDE files.  
- Local databases (`*.db`) are not committed → each developer has their own local test data.  
- Each service is isolated → can be scaled, deployed, or rebuilt independently.  

---

## ✅ Quick Test Flow

1. **Create a user**  
   ```http
   POST http://localhost:5001/api/users
   Content-Type: application/json

   { "name": "Alice", "email": "alice@example.com" }
   ```

2. **Create a product**  
   ```http
   POST http://localhost:5003/api/products
   Content-Type: application/json

   { "name": "Laptop", "price": 1200.00, "stock": 5 }
   ```

3. **Create an order**
   Use the `userId` and `productId` from the responses above:

   ```http
   POST http://localhost:5005/api/orders
   Content-Type: application/json

   { "userId": "GUID", "productId": "GUID", "quantity": 2 }
   ```

4. **Verify stock was decremented asynchronously**
   ```http
   GET http://localhost:5003/api/products/{productId}
   ```
   The `stock` value should be reduced by the ordered quantity once the message is processed.

5. **Inspect RabbitMQ (optional)**
   - Navigate to http://localhost:15672
   - Login with `guest` / `guest`
   - Check the queue generated for `OrderCreatedConsumer` (kebab-case name) to confirm published/consumed messages.

---

## 🎯 Learning Goals

By following this project, you’ll understand:
1. How to structure a microservices solution in .NET  
2. The “database per service” principle  
3. How services communicate via REST (and later async messaging)  
4. Containerization with Docker  
5. Basics of service discovery & API gateways  

---

👨‍💻 Author: *Learning Microservices step by step with AI*
