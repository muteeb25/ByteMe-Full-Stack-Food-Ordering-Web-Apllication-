# ByteMe - Full Stack Food Ordering Web Application

A modern ASP.NET Core MVC food ordering platform with user authentication, cart management, order tracking, and admin dashboard.
Live Preview:
https://bytemefoodapp.netlify.app/

## Tech Stack

- **Backend:** ASP.NET Core 8.0 MVC
- **Database:** SQLite (Development) / SQL Server (Production)
- **ORM:** Entity Framework Core 8.0
- **Frontend:** Razor Views with Bootstrap
- **Patterns:** Factory, Observer, Strategy patterns

## Features

- 🍕 Browse food menu by categories
- 🛒 Shopping cart with quantity management
- 👤 User authentication and profiles
- 📦 Order placement and tracking
- 💳 Multiple payment methods
- ⭐ Customer feedback and ratings
- 👨‍💼 Admin dashboard for management
- 📧 Newsletter subscription

## Prerequisites

- .NET 8.0 SDK
- SQL Server or SQLite
- Git

## Local Development

### 1. Clone Repository
```bash
git clone https://github.com/muteeb25/ByteMe-Full-Stack-Food-Ordering-Web-Apllication-.git
cd ByteMe-Full-Stack-Food-Ordering-Web-Apllication-
```

### 2. Setup Development Environment
```bash
cd ByteMe
dotnet restore
```

### 3. Run with SQLite (Development)
```bash
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run
```

Open browser: `http://localhost:5000`

### 4. Test Credentials
- **Admin:** admin@byteme.com / Admin@123
- **User:** user@byteme.com / User@123

## Deployment

### Option 1: Azure App Service (Recommended)

#### Prerequisites
- Azure Account
- Azure CLI installed

#### Steps
```bash
# Login to Azure
az login

# Create resource group
az group create --name ByteMeRG --location eastus

# Create App Service Plan
az appservice plan create --name ByteMe-Plan --resource-group ByteMeRG --sku B1 --is-linux

# Create Web App
az webapp create --resource-group ByteMeRG --plan ByteMe-Plan --name byteme-app --runtime "DOTNETCORE|8.0"

# Create SQL Database
az sql server create --name byteme-server --resource-group ByteMeRG --admin-user sqladmin --admin-password YourSecurePassword@123
az sql db create --resource-group ByteMeRG --server byteme-server --name ByteMeDb

# Deploy from GitHub
az webapp deployment github-actions add --resource-group ByteMeRG --name byteme-app --repo YOUR_GITHUB_USERNAME/ByteMe-Full-Stack-Food-Ordering-Web-Apllication- --branch main --runtime "dotnet|8.0"

# Configure connection string
az webapp config connection-string set --resource-group ByteMeRG --name byteme-app --connection-string-type SQLServer --settings DefaultConnection="YOUR_CONNECTION_STRING"

# Set production environment
az webapp config appsettings set --resource-group ByteMeRG --name byteme-app --settings ASPNETCORE_ENVIRONMENT="Production"
```

### Option 2: Docker Deployment (Google Cloud Run, AWS ECS, etc.)

#### Build and Test Locally
```bash
docker-compose up
```

#### Push to Container Registry
```bash
# Google Cloud
docker build -t gcr.io/YOUR_PROJECT/byteme:latest .
docker push gcr.io/YOUR_PROJECT/byteme:latest

# Deploy to Cloud Run
gcloud run deploy byteme --image gcr.io/YOUR_PROJECT/byteme:latest --region us-central1
```

#### AWS ECS
```bash
# Create ECR repository
aws ecr create-repository --repository-name byteme

# Tag and push image
docker build -t byteme:latest .
docker tag byteme:latest YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/byteme:latest
docker push YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/byteme:latest
```

### Option 3: Railway (Simplest)

1. Go to [Railway.app](https://railway.app)
2. Connect your GitHub repository
3. Railway auto-detects .NET and deploys
4. Add SQL Server add-on from Railway dashboard
5. Configure environment variables
6. Deploy automatically on push

### Option 4: Render.com

1. Go to [Render.com](https://render.com)
2. Create new Web Service
3. Connect GitHub repository
4. Select .NET 8 as runtime
5. Set Build Command: `dotnet build`
6. Set Start Command: `dotnet run --urls "http://+:10000"`
7. Add PostgreSQL database
8. Deploy

## Environment Configuration

### Development (.env or terminal)
```bash
$env:ASPNETCORE_ENVIRONMENT='Development'
# Uses SQLite - ByteMeDb.db
# Connection: appsettings.Development.json
```

### Production
```bash
$env:ASPNETCORE_ENVIRONMENT='Production'
# Uses SQL Server
# Connection: appsettings.Production.json
```

## Database Migrations

### Apply Migrations
```bash
dotnet ef database update
```

### Create New Migration
```bash
dotnet ef migrations add MigrationName
```

### Roll Back Migration
```bash
dotnet ef database update PreviousMigrationName
```

## API Endpoints (if REST API is needed)

Add to `Program.cs` for API:
```csharp
app.MapControllers();
```

## Project Structure

```
ByteMe/
├── Controllers/          # MVC controllers
├── Models/              # Data models & view models
├── Views/               # Razor templates
├── Data/                # Database context & migrations
├── Services/            # Business logic
│   ├── Patterns/        # Design pattern implementations
│   ├── Observers/       # Observer pattern
│   └── Interfaces/      # Service interfaces
├── Helpers/             # Utility classes
├── wwwroot/             # Static files (CSS, JS, images)
├── Properties/          # Launch settings
└── appsettings.*.json   # Configuration files
```

## Common Issues & Solutions

### Issue: Database Connection Failed
**Solution:** 
- Development: Delete `ByteMeDb.db` and restart
- Production: Check connection string in Azure Portal

### Issue: Port Already in Use
**Solution:**
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5000
kill -9 <PID>
```

### Issue: Migrations Not Applied
**Solution:**
```bash
dotnet ef database update --context AppDbContext
```

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## License

MIT License - see LICENSE file for details

## Contact

- GitHub: [@muteeb25](https://github.com/muteeb25)
- Email: muteeb@example.com

## Support

For issues and questions, please open an issue on GitHub.

---

**Happy Ordering! 🍔🍕**
