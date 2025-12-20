# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Part-Time Job Finding Platform** REST API built with **.NET 9.0** using **Clean Architecture** principles. The solution is structured into four projects following Domain-Driven Design:

- **PTJ.Domain**: Core domain entities, interfaces, and business logic
- **PTJ.Application**: Application services, DTOs, and mapping profiles
- **PTJ.Infrastructure**: Data access (EF Core), repositories, and external services
- **PTJ.API**: Web API controllers, middleware, and configuration

## Architecture Patterns

### Clean Architecture Layers

The project strictly follows dependency rules:
- **PTJ.API** → depends on Application, Infrastructure, Domain
- **PTJ.Infrastructure** → depends on Application, Domain
- **PTJ.Application** → depends on Domain only
- **PTJ.Domain** → no dependencies (pure business logic)

### Repository Pattern & Unit of Work

All data access goes through the **Unit of Work** pattern:
- `IRepository<T>` provides generic CRUD operations for all entities
- `IUnitOfWork` exposes typed repositories and manages transactions
- Implementation is in `PTJ.Infrastructure/Repositories/GenericRepository.cs` and `UnitOfWork.cs`

Example usage in services:
```csharp
var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);
await _unitOfWork.SaveChangesAsync();
```

### Base Entities

All entities inherit from `BaseEntity` (in `PTJ.Domain/Common/BaseEntity.cs`) which provides:
- `Id` (int, primary key)
- `CreatedAt`, `UpdatedAt` (automatic timestamps)
- `IsDeleted` (soft delete flag)
- `RowVersion` (concurrency token)

The `AppDbContext.SaveChangesAsync()` automatically sets these values. Soft deletes are implemented - entities are never physically removed.

### Result Pattern

All service methods return `Result` or `Result<T>` (defined in `PTJ.Application/Common/Result.cs`):
```csharp
public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; }
}
```

This provides consistent error handling across the API.

### Authentication & Authorization

- **JWT Bearer authentication** with access tokens (60 min) and refresh tokens (7 days)
- Configuration in `appsettings.json` under `"Jwt"` section
- `IJwtService` generates tokens, `IAuthService` handles login/register/refresh
- Role-based authorization: Users have roles (ADMIN, EMPLOYER, STUDENT) via `UserRole` join table
- Custom filter `AuthorizeCompanyOwnerFilter` ensures companies can only modify their own data

### User Registration & Role Assignment Workflow

**CRITICAL**: Follow this workflow when working with user registration and company creation:

1. **New User Registration** (`POST /api/auth/register`):
   - All new users are automatically assigned the **STUDENT** role (AuthService.cs:38-48)
   - A `Profile` entity is automatically created for every new user
   - Users start with STUDENT role only

2. **Company Registration Request** (`POST /api/Companies`):
   - Any authenticated user can submit a company registration request
   - Request is stored in `CompanyRegistrationRequests` table with status `Pending`
   - **NO Company entity is created** at this stage
   - **NO EMPLOYER role is assigned** at this stage
   - User must wait for admin approval

3. **Admin Approval Process** (`POST /api/CompanyRequests/approve`):
   - Only users with ADMIN role can approve requests
   - When approved (CompanyService.cs:214-265):
     - `Company` entity is created and linked to the user
     - `CompanyRegistrationRequest` status updated to `Approved`
     - **EMPLOYER role is assigned** to the user (lines 252-263)
     - User now has both STUDENT and EMPLOYER roles
   - If rejected, request status is set to `Rejected` with rejection reason

4. **Multi-Role Support**:
   - Users can have multiple roles simultaneously (via UserRole join table)
   - A user with EMPLOYER role still retains their STUDENT role
   - Roles are seeded in database: ADMIN (id=1), EMPLOYER (id=2), STUDENT (id=3)

## Common Development Commands

### Build and Run
```bash
# Clean solution
dotnet clean

# Build solution
dotnet build

# Run the API (from solution root)
dotnet run --project PTJ.API

# Run with specific configuration
dotnet run --project PTJ.API --configuration Release
```

### Database Migrations
```bash
# Add new migration (run from solution root)
dotnet ef migrations add <MigrationName> --project PTJ.Infrastructure --startup-project PTJ.API

# Update database
dotnet ef database update --project PTJ.Infrastructure --startup-project PTJ.API

# Remove last migration
dotnet ef migrations remove --project PTJ.Infrastructure --startup-project PTJ.API

# Generate SQL script
dotnet ef migrations script --project PTJ.Infrastructure --startup-project PTJ.API
```

### Package Management
```bash
# Add package to specific project
dotnet add PTJ.API package <PackageName>

# Remove package
dotnet remove PTJ.API package <PackageName>

# Restore packages
dotnet restore
```

## Database Configuration

- **SQL Server** via Entity Framework Core 9.0
- Connection string in `appsettings.json` under `"ConnectionStrings:Default"`
- Default: `Server=localhost;Database=PartTimeJobs;Trusted_Connection=True;TrustServerCertificate=True`
- Entity configurations use Fluent API in `PTJ.Infrastructure/Configurations/`
- `AppDbContext` applies all configurations via `ApplyConfigurationsFromAssembly`

## Key Domain Entities

### Authentication
- `User` - base user entity (Email, PasswordHash, FullName, etc.)
- `Role` - user roles (Student, Company, Admin)
- `UserRole` - many-to-many join table
- `RefreshToken` - stores refresh tokens for JWT authentication

### Company & Jobs
- `Company` - company profiles (created only after admin approval, linked to User with EMPLOYER role)
- `CompanyRegistrationRequest` - pending company registration requests (status: Pending/Approved/Rejected)
- `JobPost` - job postings with title, description, salary, location
- `JobShift` - work shifts for a job (start/end time, day of week)
- `JobPostSkill` - required skills for a job

### Student Profiles
- `Profile` - student profile (linked to User with role "Student")
- `ProfileSkill`, `ProfileExperience`, `ProfileEducation`, `ProfileCertificate` - profile details

### Applications
- `Application` - job applications from students
- `ApplicationHistory` - tracks status changes
- `ApplicationStatusLookup` - reference table for statuses (Pending, Accepted, Rejected, etc.)

### Chat (Real-time Messaging)
- `ChatConversation` - conversations between employers and students
- `ChatMessage` - individual messages within conversations
- Stored in `chat` schema for better organization
- Supports typing indicators, read receipts, and unread counts

## Service Layer Structure

Each major feature has an interface in `PTJ.Application/Services/` and implementation in `PTJ.Infrastructure/Services/`:

- `IAuthService` / `AuthService` - Authentication (login, register, refresh tokens)
- `IJobPostService` / `JobPostService` - Job posting management
- `ICompanyService` / `CompanyService` - Company profile and registration requests
- `IProfileService` / `ProfileService` - Student profile management
- `IApplicationService` / `ApplicationService` - Job application workflow
- `IFileStorageService` / `LocalFileStorageService` - File uploads (resumes, certificates)
- `ISearchService` / `SearchService` - Search functionality
- `IChatService` / `ChatService` - Real-time chat messaging

All services use `IUnitOfWork` for data access and return `Result<T>` objects.

## API Controllers

Controllers in `PTJ.API/Controllers/`:
- `AuthController` - POST /api/auth/login, /register, /refresh
- `JobPostsController` - CRUD for job posts, search
- `CompaniesController` - Company profile management, search
- `CompanyRequestsController` - Admin approval of company registrations
- `ProfilesController` - Student profile management
- `ApplicationsController` - Job application submission and tracking
- `FilesController` - File upload/download
- `ChatController` - Chat conversation and message management (REST API)

### Search Endpoints

The API provides optimized search functionality using **SQL LIKE** queries via EF Core:

**Job Post Search** (`GET /api/JobPosts/search`):
```http
GET /api/JobPosts/search?searchTerm=developer&pageNumber=1&pageSize=10&sortBy=salary&sortDescending=true
```
- Searches across: `Title`, `Description`, `Location`
- Returns only active job posts
- Supports pagination and sorting (by salary or createdAt)
- Uses `EF.Functions.Like()` for database-level LIKE queries

**Company Search** (`GET /api/Companies/search`):
```http
GET /api/Companies/search?searchTerm=tech&pageNumber=1&pageSize=10&sortDescending=true
```
- Searches across: `Name`, `Description`, `Industry`, `Address`
- Supports pagination and sorting (by createdAt)
- Uses `EF.Functions.Like()` for database-level LIKE queries

**Implementation Details**:
- Search logic in `JobPostService.SearchAsync` (JobPostService.cs:66-120) and `CompanyService.SearchAsync` (CompanyService.cs:63-102)
- Uses `IRepository.FindAsync()` with LINQ expressions that compile to SQL LIKE
- Pattern: `var searchTerm = $"%{parameters.SearchTerm}%";` then `EF.Functions.Like(field, searchTerm)`
- This approach ensures queries execute in the database, not in-memory

## Real-time Chat with SignalR

The API provides **real-time chat messaging** between EMPLOYER and STUDENT users using **SignalR**.

### SignalR Hub

**ChatHub** (`PTJ.API/Hubs/ChatHub.cs`):
- WebSocket endpoint: `ws://localhost:5000/hubs/chat` (or `wss://` for HTTPS)
- Requires JWT authentication via query string: `?access_token=YOUR_JWT_TOKEN`
- Automatic user group management based on userId

**Hub Methods** (callable from client):
```typescript
// Send a message
SendMessage(dto: SendMessageDto): void

// Mark messages as read
MarkAsRead(conversationId: number): void

// Update typing status
UpdateTyping(conversationId: number, isTyping: boolean): void

// Join/leave conversation groups
JoinConversation(conversationId: number): void
LeaveConversation(conversationId: number): void
```

**Client Events** (received from server):
```typescript
// Receive a new message
ReceiveMessage(message: ChatMessageDto): void

// Messages marked as read
MessagesMarkedAsRead(conversationId: number): void

// User typing status changed
UserTyping(userId: number, isTyping: boolean): void

// Error occurred
Error(message: string): void
```

### Chat REST API Endpoints

**Get or create conversation** (`POST /api/Chat/conversations`):
```http
POST /api/Chat/conversations
{
  "recipientId": 123,
  "jobPostId": 456  // optional
}
```

**Get user's conversations** (`GET /api/Chat/conversations`):
```http
GET /api/Chat/conversations?pageNumber=1&pageSize=20
```

**Get conversation messages** (`GET /api/Chat/conversations/{id}/messages`):
```http
GET /api/Chat/conversations/123/messages?pageNumber=1&pageSize=50
```

**Send message (HTTP alternative)** (`POST /api/Chat/messages`):
```http
POST /api/Chat/messages
{
  "conversationId": 123,  // or recipientId
  "content": "Hello!"
}
```

**Mark as read** (`POST /api/Chat/conversations/{id}/read`):
```http
POST /api/Chat/conversations/123/read
```

**Get unread count** (`GET /api/Chat/unread-count`):
```http
GET /api/Chat/unread-count
```

### Chat Features

- **One-to-one conversations** between EMPLOYER and STUDENT
- **Real-time message delivery** via SignalR WebSockets
- **Read receipts** with timestamps
- **Typing indicators** for better UX
- **Unread message counts** per conversation
- **Message pagination** for performance
- **Conversation context** with optional JobPost link
- **Fallback REST API** for clients without WebSocket support

### Implementation Architecture

**Domain Layer** (`PTJ.Domain/Entities/`):
- `ChatConversation` - conversation entity with employer/student relationship
- `ChatMessage` - message entity with read status

**Application Layer** (`PTJ.Application/`):
- `IChatService` interface
- DTOs: `ChatConversationDto`, `ChatMessageDto`, `SendMessageDto`

**Infrastructure Layer** (`PTJ.Infrastructure/Services/`):
- `ChatService` - implements business logic
- Uses `IUnitOfWork` for database operations
- Stores data in `chat` schema

**API Layer** (`PTJ.API/`):
- `ChatHub` - SignalR hub for real-time communication
- `ChatController` - REST API endpoints
- JWT authentication for both WebSocket and HTTP

### Client Connection Example (JavaScript)

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/chat", {
        accessTokenFactory: () => yourJwtToken
    })
    .withAutomaticReconnect()
    .build();

// Subscribe to events
connection.on("ReceiveMessage", (message) => {
    console.log("New message:", message);
});

connection.on("UserTyping", (userId, isTyping) => {
    console.log(`User ${userId} is ${isTyping ? 'typing' : 'not typing'}`);
});

// Start connection
await connection.start();

// Send message
await connection.invoke("SendMessage", {
    recipientId: 123,
    content: "Hello!"
});

// Update typing status
await connection.invoke("UpdateTyping", conversationId, true);
```

### CORS Configuration for SignalR

CORS is configured in Program.cs to support SignalR:
- `AllowCredentials()` is required for SignalR
- Frontend origins must be explicitly listed (not `AllowAnyOrigin()`)
- Default origins: `http://localhost:3000`, `http://localhost:5173`

## Middleware & Filters

- `GlobalExceptionMiddleware` - Catches all unhandled exceptions and returns standardized JSON responses
- `ValidationFilter` - Global validation filter (registered in Program.cs)
- `AuthorizeCompanyOwnerFilter` - Custom authorization for company-owned resources

## AutoMapper

- Mapping profiles defined in `PTJ.Application/Mapping/MappingProfile.cs`
- Maps between entities and DTOs (e.g., `User` ↔ `RegisterDto`, `JobPost` ↔ `JobPostDto`)
- Registered in DI: `builder.Services.AddAutoMapper(typeof(MappingProfile))`

## File Storage

- Local file storage configured in `appsettings.json` under `"FileStorage"`
- Default upload path: `Uploads/`
- Max file size: 10MB (10485760 bytes)
- Allowed extensions: .jpg, .jpeg, .png, .pdf, .doc, .docx
- Files are tracked in `FileEntity` table with metadata

## CORS & Swagger

- CORS policy "AllowAll" allows all origins (configured for development)
- Swagger UI available at `/swagger` in development mode
- JWT Bearer authentication configured in Swagger for testing

## Logging System

The application implements a comprehensive logging system using **Serilog** and custom database logging for tracking user activities and system errors.

### Serilog Configuration

**Packages Used**:
- `Serilog.AspNetCore` - Core Serilog integration for ASP.NET Core
- `Serilog.Sinks.MSSqlServer` - SQL Server sink (optional, for future use)
- `Serilog.Enrichers.Environment` - Adds machine name and environment info to logs

**Configuration** (`appsettings.Development.json`):
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/app-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

**Log Files**: Stored in `logs/app-YYYYMMDD.txt` with daily rotation.

### Database Logging Tables

Two dedicated tables in the `logging` schema track activities and errors:

**UserActivityLog** (`logging.UserActivityLogs`):
- Logs every HTTP request with user context
- Captures: UserId (from JWT), HTTP method, path, query string, IP address, user agent, status code, duration
- Indexed on UserId and Timestamp for fast queries
- Foreign key to Users table (nullable, for anonymous requests)

**SystemErrorLog** (`logging.SystemErrorLogs`):
- Logs all unhandled exceptions and errors
- Captures: Error level, message, exception type, stack trace, inner exception, request context, user ID
- Indexed on Level and Timestamp
- Foreign key to Users table (nullable)

### Logging Services

**IActivityLogService** / **ActivityLogService**:
```csharp
// Log user activity
await _activityLogService.LogActivityAsync(
    userId: 123,
    httpMethod: "POST",
    path: "/api/auth/login",
    queryString: null,
    ipAddress: "192.168.1.1",
    userAgent: "Mozilla/5.0...",
    statusCode: 200,
    durationMs: 45
);

// Retrieve logs with filtering
var (logs, totalCount) = await _activityLogService.GetActivityLogsAsync(
    userId: 123,
    startDate: DateTime.UtcNow.AddDays(-7),
    endDate: DateTime.UtcNow,
    pageNumber: 1,
    pageSize: 50
);
```

**IErrorLogService** / **ErrorLogService**:
```csharp
// Log system error
await _errorLogService.LogErrorAsync(
    level: "Critical",
    message: "Database connection failed",
    exception: ex,
    userId: 123,
    requestPath: "/api/jobposts",
    httpMethod: "GET",
    ipAddress: "192.168.1.1"
);

// Retrieve error logs
var (logs, totalCount) = await _errorLogService.GetErrorLogsAsync(
    level: "Critical",
    startDate: DateTime.UtcNow.AddDays(-1),
    pageNumber: 1,
    pageSize: 50
);
```

### Middleware

**RequestLoggingMiddleware**:
- Automatically logs every HTTP request
- Extracts `UserId` from JWT claims (supports `ClaimTypes.NameIdentifier`, `sub`, `userId`, `id`)
- Measures request duration using `Stopwatch`
- Captures client IP (supports `X-Forwarded-For` and `X-Real-IP` headers for proxy scenarios)
- Logs to database via `IActivityLogService`
- Registered in `Program.cs` after authentication middleware

**GlobalExceptionMiddleware** (Enhanced):
- Catches all unhandled exceptions
- Logs errors to database via `IErrorLogService` with full context:
  - Exception details (type, message, stack trace, inner exception)
  - User context (extracted from JWT)
  - Request details (path, method, query string, IP, user agent)
  - Error severity level (Warning, Error, Critical)
- Returns standardized JSON error responses
- Never throws exceptions during logging (fail-safe design)

### JWT Claims Extraction

Both middleware components extract user identity from JWT tokens using this priority order:
1. `ClaimTypes.NameIdentifier` (standard .NET claim)
2. `sub` (standard JWT claim)
3. `userId` (custom claim)
4. `id` (custom claim)

This ensures compatibility with various JWT token formats.

### Admin Logging Endpoints

**LogsController** (`/api/Logs`) - Requires `ADMIN` role:

**Get Activity Logs**:
```http
GET /api/Logs/activities?userId=123&startDate=2024-01-01&pageNumber=1&pageSize=50
```

**Get Error Logs**:
```http
GET /api/Logs/errors?level=Critical&startDate=2024-01-01&pageNumber=1&pageSize=50
```

**Get Activity Statistics**:
```http
GET /api/Logs/activities/stats?startDate=2024-01-01&endDate=2024-01-31
```
Returns:
- Total requests, successful/failed counts
- Unique users, anonymous requests
- Average response time
- Top 10 most accessed paths

**Get Error Statistics**:
```http
GET /api/Logs/errors/stats?startDate=2024-01-01&endDate=2024-01-31
```
Returns:
- Error counts by level (Critical/Error/Warning)
- Top 10 exception types
- Top 10 error-prone endpoints

### Client IP Detection

The logging system intelligently detects client IP addresses considering proxy/load balancer scenarios:
1. Check `X-Forwarded-For` header (takes first IP from comma-separated list)
2. Check `X-Real-IP` header
3. Fallback to `HttpContext.Connection.RemoteIpAddress`

This ensures accurate IP logging in both development and production environments.

### Best Practices

- **Logging Never Fails**: Both logging services wrap operations in try-catch blocks to prevent logging errors from crashing the application
- **Performance**: Activity logs use async operations and don't block request processing
- **Security**: Sensitive data (passwords, tokens) should never be logged
- **Retention**: Consider implementing log retention policies to manage database size
- **Monitoring**: Use log statistics endpoints to identify performance bottlenecks and error patterns

## Important Notes

- All timestamps use **UTC** (`DateTime.UtcNow`)
- Password hashing uses **BCrypt** (via BCrypt.Net-Next package)
- Concurrency conflicts handled via `RowVersion` (optimistic concurrency)
- Soft deletes are enforced - check `IsDeleted` flag in queries
- The `AppDbContext` automatically handles soft deletes in `SaveChangesAsync`
