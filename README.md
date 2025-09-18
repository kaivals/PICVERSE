# PicVerse - Social Media Platform
## ASP.NET Core 9 + Angular 19 Full-Stack Blueprint

### Overview
PicVerse is a modern social media platform built with ASP.NET Core 9 Web API backend and Angular 19 frontend, featuring real-time messaging, OTP authentication, and comprehensive social features.

### Technology Stack

#### Backend
- **ASP.NET Core 9** - Web API
- **Entity Framework Core 9** - ORM
- **SQL Server/MySQL** - Database
- **SignalR** - Real-time communication
- **JWT** - Authentication
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Logging

#### Frontend
- **Angular 19** - Frontend framework
- **Angular Material** - UI components
- **RxJS** - Reactive programming
- **NgRx** - State management
- **Socket.io-client** - Real-time communication

### Project Structure

```
PicVerse/
├── Backend/
│   ├── PicVerse.API/
│   ├── PicVerse.Core/
│   ├── PicVerse.Infrastructure/
│   └── PicVerse.Tests/
└── Frontend/
    ├── src/
    │   ├── app/
    │   │   ├── core/
    │   │   ├── shared/
    │   │   ├── features/
    │   │   └── layouts/
    │   ├── assets/
    │   └── environments/
```

### Database Schema

#### Core Tables
- **Users** - User profiles and authentication
- **Posts** - User posts and media
- **Likes** - Post likes tracking
- **Comments** - Post comments
- **Follows** - User following relationships
- **Chats** - Private messaging
- **Messages** - Chat messages
- **OtpCodes** - OTP verification
- **AdminActions** - Moderation logs

### Key Features Implementation

#### 1. OTP-Based Authentication
- Email/SMS OTP generation and verification
- JWT token management with refresh tokens
- Session handling and security

#### 2. Real-time Features
- Live messaging with SignalR
- Real-time notifications
- Online user status

#### 3. Social Features
- Post creation with media upload
- Like/unlike functionality
- Commenting system
- User following/followers
- User search and discovery

#### 4. Admin Panel
- User management
- Content moderation
- Analytics dashboard
- System monitoring

### Security Best Practices
- JWT with refresh token rotation
- Rate limiting and throttling
- Input validation and sanitization
- CORS configuration
- SQL injection prevention
- XSS protection

### Deployment on Azure
- Azure App Service for API
- Azure SQL Database
- Azure Blob Storage for media
- Azure SignalR Service
- Azure Application Insights

### Getting Started

#### Prerequisites
- .NET 9 SDK
- Node.js 18+
- SQL Server/MySQL
- Visual Studio 2022 or VS Code

#### Backend Setup
```bash
cd Backend
dotnet restore
dotnet ef database update
dotnet run --project PicVerse.API
```

#### Frontend Setup
```bash
cd Frontend
npm install
ng serve
```

### API Endpoints Overview

#### Authentication
- POST /api/auth/send-otp
- POST /api/auth/verify-otp
- POST /api/auth/refresh-token
- POST /api/auth/logout

#### Users
- GET /api/users/profile
- PUT /api/users/profile
- GET /api/users/search
- POST /api/users/follow/{id}

#### Posts
- GET /api/posts
- POST /api/posts
- PUT /api/posts/{id}
- DELETE /api/posts/{id}
- POST /api/posts/{id}/like

#### Comments
- GET /api/posts/{id}/comments
- POST /api/posts/{id}/comments
- PUT /api/comments/{id}
- DELETE /api/comments/{id}

#### Messaging
- GET /api/chats
- POST /api/chats
- GET /api/chats/{id}/messages
- POST /api/chats/{id}/messages

This blueprint provides the foundation for building a scalable, secure social media platform. Each component is designed with best practices and can be extended based on specific requirements.