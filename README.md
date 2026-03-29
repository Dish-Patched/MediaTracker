# MediaTracker

A personal media tracking web app built with ASP.NET Core. Track everything you watch, read, and play: movies, TV shows, books, and video games, all in one place.

## Features

- **Account system** — register and log in to your personal collection
- **Multi-media support** — track movies, TV shows, books, and video games
- **Auto-filled details** — search by title and get cover art, description, release date, genre, director/author/developer, and ratings pulled automatically
- **Status tracking** — mark items as Plan To, In Progress, Completed, or Dropped
- **Personal ratings** — rate items 1–10 and add your own notes
- **Dashboard** — view your full collection with tabs per media type and at-a-glance stats

## Tech Stack

- **ASP.NET Core 8 MVC** — web framework
- **Entity Framework Core** — database ORM
- **SQLite** (local) / **PostgreSQL** (production) — database
- **ASP.NET Core Identity** — authentication
- **Bootstrap 5** — UI with a dark theme

## APIs Used

| API | Used For |
|-----|----------|
| [TMDB](https://www.themoviedb.org/documentation/api) | Movies and TV shows |
| [Google Books](https://developers.google.com/books) | Books |
| [RAWG](https://rawg.io/apidocs) | Video games |

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- TMDB API key (free at [themoviedb.org](https://www.themoviedb.org/settings/api))
- RAWG API key (free at [rawg.io/apidocs](https://rawg.io/apidocs))

### Setup

1. Clone the repo
```bash
git clone https://github.com/Dish-Patched/MediaTracker.git
cd MediaTracker
```

2. Add your API keys to `appsettings.json`
```json
"ApiKeys": {
  "Tmdb": "your_tmdb_key",
  "Rawg": "your_rawg_key"
}
```

3. Run the app
```bash
dotnet run
```

4. Navigate to `https://localhost:{port}` — you'll be taken to the register page

## Deployment

The app is configured for deployment on [Railway](https://railway.app). Set the following environment variables:

| Variable | Value |
|----------|-------|
| `DATABASE_URL` | Provided automatically by Railway's PostgreSQL plugin |
| `ApiKeys__Tmdb` | Your TMDB API key |
| `ApiKeys__Rawg` | Your RAWG API key |
