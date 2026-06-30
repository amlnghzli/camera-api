# Camera API - Setup Instructions

## Prerequisites
- .NET 8 SDK installed
- SQL Server Express or Developer Edition
- Git

## Installation Steps

### Step 1: Clone and Navigate to Project
```bash
cd camera-api
```

### Step 2: Update Connection String
1. Open `appsettings.json`
2. Update the `DefaultConnection` string to match your SQL Server instance:
   - For local SQL Server with Windows Authentication:
     ```
     "DefaultConnection": "Server=.;Database=CameraMarketDb;Trusted_Connection=true;Encrypt=false;"
     ```
   - For SQL Server with username/password:
     ```
     "DefaultConnection": "Server=localhost;Database=CameraMarketDb;User Id=sa;Password=YourPassword;Encrypt=false;"
     ```

### Step 3: Create Database and Apply Migrations
```bash
# Restore packages
dotnet restore

# Create and apply initial migration
dotnet ef database update
```

### Step 4: Run the Application
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5175`
- HTTPS: `https://localhost:5174`

Swagger UI: `https://localhost:5174/swagger`

### Step 5: Configure React Frontend
In your React app, add API calls to `http://localhost:5174` or `https://localhost:5174`

Example fetch call:
```javascript
const response = await fetch('https://localhost:5174/api/cameras');
const cameras = await response.json();
```

## Connecting to React Frontend

Update your React component to call the API:

```jsx
import { useEffect, useState } from 'react';

function CameraList() {
  const [cameras, setCameras] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch('https://localhost:5174/api/cameras')
      .then(res => res.json())
      .then(data => {
        setCameras(data);
        setLoading(false);
      })
      .catch(err => {
        console.error('Error fetching cameras:', err);
        setLoading(false);
      });
  }, []);

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      {cameras.map(camera => (
        <div key={camera.id}>
          <h3>{camera.brand} {camera.model}</h3>
          <p>${camera.price}</p>
        </div>
      ))}
    </div>
  );
}

export default CameraList;
```

## Docker Deployment

### Using Docker Compose (Easiest)

```bash
# Build and start both SQL Server and API
docker-compose up -d

# API will be available at http://localhost:5174
```

### Manual Docker Build

```bash
docker build -t camera-api .
docker run -p 5174:8080 camera-api
```

## Troubleshooting

### Connection String Errors
- Verify SQL Server is running
- Check server name (use `.` for local server, or `.\SQLEXPRESS` for Express Edition)
- Ensure database name is correct

### HTTPS Certificate Issues (Development)
If you encounter certificate errors, either:
1. Trust the dev certificate: `dotnet dev-certs https --trust`
2. Or use HTTP during development (port 5175)

### Database Migration Issues
```bash
# Remove last migration if needed
dotnet ef migrations remove

# Check migration status
dotnet ef migrations list
```

## API Documentation

Once running, visit: `https://localhost:5174/swagger` to see interactive API documentation.

## Next Steps

1. Test API endpoints with Swagger UI
2. Connect React frontend to this API
3. Implement authentication/authorization as needed
4. Add more business logic and validations
5. Set up CI/CD pipeline for deployment
