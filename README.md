# Camera Market API - .NET Core Microservice

A .NET 8 Core REST API microservice for managing camera listings with Entity Framework Core and SQL Server.

## Prerequisites

- .NET 8 SDK
- SQL Server (local or remote)
- Visual Studio, VS Code, or Rider

## Getting Started

### 1. Update Connection String
Edit `appsettings.json` to point to your SQL Server instance:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=CameraMarketDb;Trusted_Connection=true;Encrypt=false;"
}
```

### 2. Create Database (if using SQL Server with Migrations)
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Run the API
```bash
dotnet run
```

The API will start on `http://localhost:5174` (HTTPS) or `https://localhost:5174`

## API Endpoints

### GET /api/cameras
Get all cameras with optional pagination
- Query Parameters:
  - `skip`: Number of records to skip
  - `take`: Number of records to take
  - `activeOnly`: Filter only active cameras (default: true)

**Example:**
```bash
GET /api/cameras?skip=0&take=10
```

### GET /api/cameras/{id}
Get a specific camera by ID

### GET /api/cameras/search/{term}
Search cameras by brand, model, or type

### POST /api/cameras
Create a new camera
**Request Body:**
```json
{
  "brand": "Canon",
  "model": "EOS R5",
  "type": "Mirrorless",
  "price": 3499.99,
  "sensor": "Full Frame",
  "megapixels": 45,
  "resolution": "8192 x 5464",
  "is4KCapable": true,
  "description": "Professional mirrorless camera",
  "imageUrl": "https://example.com/image.jpg"
}
```

### PUT /api/cameras/{id}
Update an existing camera

### DELETE /api/cameras/{id}
Delete a camera

## Database Schema

### Camera Table
- `Id` (int, Primary Key)
- `Brand` (string, max 100)
- `Model` (string, max 200)
- `Type` (string, max 50)
- `Price` (decimal)
- `Sensor` (string, max 200)
- `Megapixels` (int)
- `Resolution` (string, max 100)
- `Is4KCapable` (bool)
- `Description` (string, max 1000)
- `ImageUrl` (string, max 500)
- `CreatedAt` (datetime)
- `UpdatedAt` (datetime)
- `IsActive` (bool)

## CORS Configuration

The API is configured to accept requests from:
- `http://localhost:5173` (Vite dev server)
- `http://localhost:3000` (Alternative React dev port)

Update `Program.cs` to add more origins as needed.

## Docker Support

To build and run with Docker:

```bash
docker build -t camera-api .
docker run -p 5174:8080 camera-api
```

## Technologies

- **.NET 8** - Framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **Swagger/OpenAPI** - API Documentation

## Development

### Entity Framework Core Commands

Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

Remove last migration:
```bash
dotnet ef migrations remove
```

## License

MIT
