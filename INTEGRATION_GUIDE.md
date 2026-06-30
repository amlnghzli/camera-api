# React + .NET Core Integration Guide

This guide explains how to connect your React frontend (camera-market-app) with the .NET Core API microservice (camera-api).

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│         React Frontend (Vite)                       │
│    http://localhost:5173                            │
│  - Components                                       │
│  - State Management                                 │
│  - UI                                               │
└──────────────────┬──────────────────────────────────┘
                   │
                   │ HTTP/HTTPS Requests
                   ↓
┌─────────────────────────────────────────────────────┐
│   .NET Core API (ASP.NET Core)                      │
│   https://localhost:5174                            │
│  - REST Endpoints                                   │
│  - Database Access (EF Core)                        │
│  - Business Logic                                   │
└──────────────────┬──────────────────────────────────┘
                   │
                   │ Database Queries
                   ↓
              ┌─────────────┐
              │  SQL Server │
              │  Database   │
              └─────────────┘
```

## Setup Checklist

- [ ] .NET 8 SDK installed
- [ ] SQL Server running locally
- [ ] API database created and migrations applied
- [ ] API running on https://localhost:5174
- [ ] React app running on http://localhost:5173
- [ ] CORS configured in API (already done)
- [ ] React components updated to call API

## Integration Steps

### 1. Create API Service in React

Create `src/services/cameraService.js`:

```javascript
const API_BASE_URL = 'https://localhost:5174/api';

export const cameraService = {
  // Fetch all cameras
  getAllCameras: async (skip = 0, take = 10) => {
    const response = await fetch(
      `${API_BASE_URL}/cameras?skip=${skip}&take=${take}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );
    if (!response.ok) throw new Error('Failed to fetch cameras');
    return response.json();
  },

  // Get single camera
  getCameraById: async (id) => {
    const response = await fetch(`${API_BASE_URL}/cameras/${id}`);
    if (!response.ok) throw new Error('Failed to fetch camera');
    return response.json();
  },

  // Search cameras
  searchCameras: async (term) => {
    const response = await fetch(`${API_BASE_URL}/cameras/search/${term}`);
    if (!response.ok) throw new Error('Search failed');
    return response.json();
  },

  // Create camera
  createCamera: async (camera) => {
    const response = await fetch(`${API_BASE_URL}/cameras`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(camera),
    });
    if (!response.ok) throw new Error('Failed to create camera');
    return response.json();
  },

  // Update camera
  updateCamera: async (id, camera) => {
    const response = await fetch(`${API_BASE_URL}/cameras/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(camera),
    });
    if (!response.ok) throw new Error('Failed to update camera');
    return response.json();
  },

  // Delete camera
  deleteCamera: async (id) => {
    const response = await fetch(`${API_BASE_URL}/cameras/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete camera');
  },
};
```

### 2. Use Service in Components

Example `CameraList.jsx`:

```jsx
import { useEffect, useState } from 'react';
import { cameraService } from '../services/cameraService';

export default function CameraList() {
  const [cameras, setCameras] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchCameras = async () => {
      try {
        setLoading(true);
        const data = await cameraService.getAllCameras(0, 20);
        setCameras(data);
        setError(null);
      } catch (err) {
        setError(err.message);
        console.error('Error fetching cameras:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchCameras();
  }, []);

  if (loading) return <div>Loading cameras...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="camera-list">
      <h1>Available Cameras</h1>
      <div className="grid">
        {cameras.map(camera => (
          <div key={camera.id} className="camera-card">
            <img src={camera.imageUrl} alt={camera.model} />
            <h3>{camera.brand} {camera.model}</h3>
            <p className="type">{camera.type}</p>
            <p className="specs">
              {camera.megapixels}MP • {camera.sensor}
            </p>
            {camera.is4KCapable && <span className="badge">4K</span>}
            <p className="price">${camera.price.toFixed(2)}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
```

### 3. HTTPS Certificate Setup (Development)

For development, you need to handle HTTPS. The API uses self-signed certificates by default.

#### Option A: Trust the Dev Certificate
```bash
# In camera-api directory
dotnet dev-certs https --trust
```

#### Option B: Disable SSL Verification (Dev Only)
Add to your fetch calls:
```javascript
const response = await fetch(url, {
  method: 'GET',
  headers: { 'Content-Type': 'application/json' },
  // This is needed during development with self-signed certs
  // DO NOT use in production!
});
```

### 4. Environment-Specific Configuration

Create `src/config.js`:

```javascript
const API_BASE_URL = 
  process.env.NODE_ENV === 'production'
    ? 'https://api.yourdomain.com'
    : 'https://localhost:5174/api';

export default API_BASE_URL;
```

Then use it:
```javascript
import API_BASE_URL from '../config';

const response = await fetch(`${API_BASE_URL}/cameras`);
```

### 5. Handle CORS in Vite (if needed)

Update `vite.config.js`:

```javascript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:5174',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, '/api'),
        secure: false, // For dev with self-signed certs
      }
    }
  }
})
```

## Running Both Services

### Terminal 1: Start .NET API
```bash
cd ../camera-api
dotnet run
# API runs on https://localhost:5174
```

### Terminal 2: Start React App
```bash
cd camera-market-app
npm run dev
# Frontend runs on http://localhost:5173
```

## Testing Integration

1. Ensure API Swagger is accessible: `https://localhost:5174/swagger`
2. Test endpoints in Swagger
3. Check React browser console for any fetch errors
4. Use browser DevTools Network tab to monitor API calls

## Common Issues & Solutions

### CORS Error
```
Access to XMLHttpRequest at 'https://localhost:5174/api/cameras' 
from origin 'http://localhost:5173' has been blocked by CORS policy
```

**Solution:** CORS is already configured in the API. If error persists:
- Verify API is running
- Check the origins in `Program.cs`
- Clear browser cache

### SSL/Certificate Error
```
failed to fetch - The operation couldn't be completed
```

**Solution:** 
- Trust the dev certificate: `dotnet dev-certs https --trust`
- Or modify vite.config.js to use proxy

### 404 Not Found
- Verify API is running on correct port (5174)
- Check endpoint URL in cameraService.js
- Verify database has data (check Swagger UI)

## Deployment Considerations

### Production Setup
1. Deploy API to Azure, AWS, or on-premises server
2. Update React API_BASE_URL to production endpoint
3. Configure HTTPS with valid certificates
4. Set up authentication/authorization
5. Configure production CORS origins
6. Set up CI/CD pipeline

### Environment Variables
Create `.env` files for different environments:

**.env.development**
```
VITE_API_URL=https://localhost:5174/api
```

**.env.production**
```
VITE_API_URL=https://api.yourdomain.com/api
```

Use in code:
```javascript
const API_BASE_URL = import.meta.env.VITE_API_URL;
```

## Performance Tips

1. **Implement pagination** - Load cameras in batches
2. **Use caching** - Cache camera data in React state
3. **Lazy loading** - Load images on demand
4. **Pagination in API** - Use skip/take parameters

## Next Steps

1. Create additional API endpoints as needed
2. Add user authentication
3. Implement shopping cart functionality
4. Add filters (price, camera type, etc.)
5. Set up backend data validation
6. Add comprehensive error handling

## Resources

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [React Documentation](https://react.dev)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Vite Documentation](https://vitejs.dev)
