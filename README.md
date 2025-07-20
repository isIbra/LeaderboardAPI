# Leaderboard API

A generic, reusable leaderboard API built with ASP.NET Core 8.0 and MongoDB. Perfect for games and applications requiring score tracking and player rankings.

## Docker Setup

This project is containerized using Docker. The setup includes:
- ASP.NET Core 8.0 API
- MongoDB database

### Prerequisites

- Docker
- Docker Compose

### Quick Start

1. Clone the repository

2. Navigate to the project directory:
   ```bash
   cd Leaderboard.API
   ```

3. Set up environment variables:

   **Option A - Using an .env file:**
   ```bash
   # Create .env file from the example
   cp .env.example .env
   # Edit the .env file and set your secrets
   nano .env  # or use your preferred editor
   ```

   **Option B - Passing directly on command line:**
   ```bash
   Leaderboard_SECRET_KEY="your-secret-key" Leaderboard_GAME_PARTNER_SECRET="your-partner-secret" docker-compose up -d --build
   ```

4. Build and start the containers (if using Option A):
   ```bash
   docker-compose up -d --build
   ```

5. The API will be available at:
   - http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger

6. To stop the containers:
   ```bash
   docker-compose down
   ```

## Configuration

### Environment Variables

For security, sensitive information is configured through environment variables:

- `Leaderboard_SECRET_KEY`: JWT signing key (required)
- `Leaderboard_GAME_PARTNER_SECRET`: Secret for verifying game partner requests (required)
- `Leaderboard_TOKEN_EXPIRATION_MINUTES`: Token expiration time in minutes (optional, default: 60)
- `MONGODB_URI`: MongoDB connection string (optional, overrides configuration files)

### Configuration Files

The project uses different configuration files for different environments:

- `appsettings.json` - Default settings (uses MongoDB at "mongodb:27017")
- `appsettings.Development.json` - Development settings (uses MongoDB at "localhost:27017")
- `appsettings.Production.json` - Production settings (uses MongoDB at "mongodb:27017")

## API Endpoints

### Authentication

All endpoints require JWT authentication. See the [Authentication Guide](AUTH-README.md) for details.

### Leaderboard Endpoints

#### Get Leaderboard
```
GET /api/v1/Leaderboard/leaderboard
```
Retrieves the leaderboard with optional pagination.

**Parameters:**
- `limit` (optional): Number of entries to return (default: 10)
- `skip` (optional): Number of entries to skip (default: 0)

**Response Example:**
```json
{
  "entries": [
    {
      "userId": "user123",
      "name": "Player One",
      "score": 5000,
      "imageUrl": "https://example.com/avatar.jpg",
      "deviceType": "Mobile",
      "createdAt": "2023-04-15T12:30:45Z",
      "totalGamesPlayed": 15,
      "recentScores": [
        {
          "score": 4500,
          "timestamp": "2023-04-14T10:20:30Z",
          "deviceType": "Mobile"
        }
      ]
    }
  ],
  "totalCount": 100
}
```

#### Get Filtered Leaderboard
```
GET /api/v1/Leaderboard/leaderboard/filtered
```
Retrieves a filtered leaderboard with various filter options.

**Parameters:**
- `limit` (optional): Number of entries to return (default: 10)
- `skip` (optional): Number of entries to skip (default: 0)
- `ascending` (optional): Sort order (default: false - descending)
- `userIds` (optional): Filter by specific user IDs
- `deviceType` (optional): Filter by device type
- `startDate` (optional): Filter by date range start
- `endDate` (optional): Filter by date range end
- `minScore` (optional): Filter by minimum score
- `maxScore` (optional): Filter by maximum score

#### Get User Highest Score
```
GET /api/v1/Leaderboard/leaderboard/user-highest-score
```
Retrieves the highest score of the current authenticated user.

**Response Example:**
```json
{
  "userId": "user123",
  "name": "Player One",
  "score": 5000,
  "createdAt": "2023-04-15T12:30:45Z",
  "totalGamesPlayed": 15,
  "recentScores": [
    {
      "score": 4800,
      "timestamp": "2023-04-15T10:20:30Z",
      "deviceType": "Mobile"
    }
  ]
}
```

#### Submit Score
```
POST /api/v1/Leaderboard/leaderboard/score/submit
```
Submits a new score to the leaderboard.

**Request Body:**
```json
{
  "name": "Player One",
  "score": 5000,
  "imageUrl": "https://example.com/avatar.jpg"
}
```

**Headers:**
- `X-Device-Type` (optional): The device type used (e.g., "Mobile", "Desktop")

**Response Example:**
```json
{
  "userId": "user123",
  "name": "Player One",
  "score": 5000,
  "imageUrl": "https://example.com/avatar.jpg",
  "highestScore": 5000,
  "totalGamesPlayed": 15,
  "createdAt": "2023-04-15T12:30:45Z"
}
```

### User Endpoints

#### Get User Stats
```
GET /api/v1/Leaderboard/user/stats
```
Retrieves statistics for the current authenticated user.

**Response Example:**
```json
{
  "userId": "user123",
  "name": "Player One",
  "imageUrl": "https://example.com/avatar.jpg",
  "highestScore": 5000,
  "totalGamesPlayed": 15,
  "lastPlayed": "2023-04-15T12:30:45Z",
  "isNewUser": false,
  "recentScores": [
    {
      "score": 4800,
      "timestamp": "2023-04-15T10:20:30Z",
      "deviceType": "Mobile"
    }
  ]
}
```

### Analytics Endpoints

#### Get Retention Metrics
```
GET /api/v1/analytics/retention/metrics
```
Provides detailed retention rates at different intervals (1 day, 3 days, 7 days, 30 days).

**Parameters:**
- `startDate` (optional): The start date for the analysis period
- `endDate` (optional): The end date for the analysis period

#### Compare New vs Returning Players
```
GET /api/v1/analytics/retention/compare
```
Provides metrics comparing new players versus returning players.

## Development

For local development outside of Docker:

1. Ensure you have .NET 8.0 SDK installed
2. Install and run MongoDB locally
3. Run the application with Development environment:
   ```bash
   # Windows
   set ASPNETCORE_ENVIRONMENT=Development
   dotnet run

   # Linux/MacOS
   ASPNETCORE_ENVIRONMENT=Development dotnet run
   ```

## Features

- **Generic Design**: No hardcoded game types or categories
- **JWT Authentication**: Secure token-based authentication
- **Score Tracking**: Submit and retrieve player scores
- **Leaderboards**: Ranked lists with filtering and pagination
- **Player Analytics**: Retention metrics and player statistics
- **Docker Ready**: Easy deployment with Docker Compose
- **Health Checks**: Built-in health monitoring
- **Swagger Documentation**: Interactive API documentation

## Health Checks

Both the API and MongoDB services have health checks configured:
- API health: http://localhost:8080/health/status
- MongoDB health: Automatically checked inside the container

## Data Persistence

MongoDB data is persisted using a named volume (`mongodb_data`). This ensures your data survives container restarts.