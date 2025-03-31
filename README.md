# BookStoreTester

A .NET web application for managing bookstore inventory.

## 🚀 Deployment

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) (optional)

### Local Development
```bash
cd BookStoreTester
dotnet run
```
Access: `http://localhost:5000`

### Docker Build
```bash
docker build -t bookstore-app .
docker run -p 5000:80 bookstore-app
```

### Render.com Deployment
1. Connect your GitHub repository
2. Configure:
   - **Environment**: Docker
   - **Root Directory**: `BookStoreTester/`
   - **Dockerfile Path**: `./Dockerfile`
3. Add environment variables if needed

## 📂 Project Structure
```
BookStoreTester/
├── Controllers/
├── Models/
├── Views/
├── Dockerfile          # Deployment config
├── BookStoreTester.csproj
└── appsettings.json   # Configuration
```

## ⚙️ Configuration
Set these in `appsettings.json` or Render environment variables:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-database-connection"
  }
}
```

## 📄 License
MIT