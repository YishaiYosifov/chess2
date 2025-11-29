![logo-text](frontend/public/assets/logo-text.svg)

# Chess: The Anarchy Update

[Terms Of Service](https://anarchychess.org/tos) | [Privacy Policy](https://anarchychess.org/privacy) | [License](LICENSE)

**Chess: The Anarchy Update** is a chess website that takes all the stupid ideas from [r/AnarchyChess](https://reddit.com/r/anarchychess) and turns them a real, balanced, chess variant.

# Features

-   **New Pieces & Rules:** Knook, Checker, Traitor Rook, Antiqueen, Underage Pawn, King Capture, King Touch = Draw, Forced En Passant, Long Passant, Il Vaticano, Omnipotent Pawn, Vertical Castling, Knooklear Fusion, Queen Beta Decay.\
    _See the [full guide](https://anarchychess.org/guide) for detailed explanations of all pieces and rules_

-   **Player Profiles:** Track ratings, game history and progress for each time control
-   **Social Features:** Stars, blocks, in-game chat, leaderboards, direct challenges
-   **Matchmaking:** Seek a game in any time control, all at once, rated or casual. Your seek is also displayed as an open seek, allowing players to accept it directly without having to go through the pool
-   **Daily Quests:** Complete daily quests to climb the leaderboards and build a streak

# Screenshots

<img src="screenshots/checker.png" alt="Checker" width="400">
<img src="screenshots/il-vaticano.png" alt="Il Vaticano" width="400">
<img src="screenshots/knooklear-fusion.png" alt="Knooklear Fusion" width="400">
<img src="screenshots/long-passant.png" alt="Long Passant" width="400">

# Tech Stack

-   **Backend:** C# With ASP.NET Core, structured with Orleans
-   **Frontend:** Next.js + Typescript, styled with Tailwind
-   **Database & Storage:** Currently configured for PostgreSQL and Azure Blob Storage. Other SQL databases and blob storage providers can be used by installing the appropriate EF Core and FluentStorage packages.

# Installation / Running Locally

## Backend Setup

1. Navigate to the backend directory

```bash
cd backend/AnarchyChess.Api
```

2. Restore dependencies

```bash
dotnet restore
```

3. Initialize & set secrets

```bash
dotnet user-secrets init

dotnet user-secrets set "Authentication:Google:ClientId" "<client-id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<client-secret>"

dotnet user-secrets set "Authentication:Discord:ClientId" "<client-id>"
dotnet user-secrets set "Authentication:Discord:ClientSecret" "<client-secret>"

dotnet user-secrets set "Authentication:JWTSecret" "<jwt-secret>"

dotnet user-secrets set "Services:Database:ConnString" "<connection-string>"
dotnet user-secrets set "Services:BlobStorage:ConnString" "<connection-string>"
```

4. Setup appsettings file

```bash
cp appsettings.Example.json appsettings.Development.json
```

This creates a development configuration file. You don't need to change anything in order to run.

5. Run the backend server

```bash
dotnet run
```

## Frontend Setup

1. Navigate to the frontend directory

```bash
cd frontend
```

2. Install dependencies

```bash
npm install
```

3. Setup environment variables

    Create a .env file:

```
NEXT_PUBLIC_API_URL="https://localhost:7266"
```

4. Run the development server:

```bash
npm run dev
```

## Database Setup

1. Create a database

```sql
CREATE DATABASE anarchychess;
```

2. Set the connection string

```bash
dotnet user-secrets set "Services:Database:ConnString" "<connection-string>"
```

3. Run Orleans SQL Setup Scripts

Run these scripts in order against your database:

```bash
backend/Scripts/Orleans
|- 001-query.sql
|- 002-reminders.sql
|- 003-storage.sql
```

4. Apply EF Core migrations

```bash
cd backend/AnarchyChess.Api
dotnet ef migrations add Initial
```

In development the backend automatically applies migrations on startup. Otherwise, run

```bash
dotnet ef database update
```

# Running Tests

## Backend

There are 3 test projects:

-   AnarchyChess.Api.Unit
-   AnarchyChess.Api.Integration
-   AnarchyChess.Api.Functional

To run all backend tests:

```bash
cd backend
dotnet test AnarchyChess.Api.sln
```

## Frontend

The frontend uses Vitest for testing. Run all tests with:

```bash
cd frontend
npm run test
```
