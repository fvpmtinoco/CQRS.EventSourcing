# CQRS.EventSourcing

A proof-of-concept implementing **CQRS** (Command Query Responsibility Segregation) with **Event Sourcing** using .NET 10, Kafka, MongoDB, SQL Server and PostgreSQL.

## Architecture

```
                         ┌──────────────┐
                         │    Client    │
                         └──────┬───────┘
                      ┌─────────┴─────────┐
                      │                   │
               ┌──────▼──────┐     ┌──────▼──────┐
               │  Post.Cmd   │     │ Post.Query  │
               │     API     │     │     API     │
               │  (Write)    │     │   (Read)    │
               └──────┬──────┘     └──────▲──────┘
                      │                   │
               ┌──────▼──────┐     ┌──────┴──────┐
               │   MongoDB   │     │ PostgreSQL/ │
               │ Event Store │     │ SQL Server  │
               └──────┬──────┘     └──────▲──────┘
                      │                   │
                      │  ┌────────────┐   │
                      └──► Apache     ├───┘
                         │   Kafka    │
                         └────────────┘
```

**Write side** receives commands, validates them through the `PostAggregate`, persists domain events to MongoDB, and publishes them to Kafka.

**Read side** consumes events from Kafka and projects them into a denormalized read model (PostgreSQL or SQL Server) optimized for queries.

## Project Structure

```
src/
├── CQRS.Core/                      # Shared abstractions (AggregateRoot, interfaces)
├── Post.Common/                    # Shared events and DTOs
├── Post.Cmd/
│   ├── Post.Cmd.Api/               # Command API controllers
│   ├── Post.Cmd.Domain/            # PostAggregate (event sourcing)
│   └── Post.Cmd.Infrastructure/    # EventStore, Kafka producer, MongoDB repo
└── Post.Query/
    ├── Post.Query.Api/             # Query API controllers
    ├── Post.Query.Domain/          # Read model entities and repository interfaces
    └── Post.Query.Infrastructure/  # Event handlers, Kafka consumer, EF Core repos
```

## Tech Stack

| Component       | Technology                          |
|-----------------|-------------------------------------|
| Runtime         | .NET 10                             |
| Event Store     | MongoDB                             |
| Read Database   | PostgreSQL / SQL Server 2022 Express |
| Messaging       | Apache Kafka (KRaft mode)           |
| ORM             | Entity Framework Core (query side)  |
| API Docs        | OpenAPI + Scalar UI                 |

## API Endpoints

### Command Side (port 8080)

| Method   | Endpoint                        | Description        |
|----------|---------------------------------|--------------------|
| `POST`   | `/api/v1/newpost`               | Create a new post  |
| `PUT`    | `/api/v1/editMessage/{id}`      | Edit post message  |
| `PUT`    | `/api/v1/likepost/{id}`         | Like a post        |
| `PUT`    | `/api/v1/addcomment/{id}`       | Add a comment      |
| `PUT`    | `/api/v1/editcomment/{id}`      | Edit a comment     |
| `PUT`    | `/api/v1/removecomment/{id}`    | Remove a comment   |
| `DELETE` | `/api/v1/deletepost/{id}`       | Delete a post      |
| `POST`   | `/api/v1/restoredb`             | Restore read DB    |

### Query Side (port 8060)

| Method | Endpoint                                 | Description                    |
|--------|------------------------------------------|--------------------------------|
| `GET`  | `/api/v1/posts`                          | List all posts                 |
| `GET`  | `/api/v1/posts/{id}`                     | Get post by ID                 |
| `GET`  | `/api/v1/posts/by-author/{author}`       | Filter posts by author         |
| `GET`  | `/api/v1/posts/with-comments`            | Posts that have comments        |
| `GET`  | `/api/v1/posts/with-likes/{minLikes}`    | Posts with at least N likes    |

API documentation is available at `/scalar/v1` on each service.

## Running

### Prerequisites

- Docker & Docker Compose
- Create the external network: `docker network create smpost`

### Start all services

```bash
docker compose up -d
```

This starts Kafka, MongoDB, SQL Server, PostgreSQL, the Command API and the Query API. The Query API automatically creates the read database on first run.

### Infrastructure only

To run just the infrastructure (Kafka, MongoDB, SQL Server, PostgreSQL) and develop locally:

```bash
docker compose -f docker-compose.yml up -d
```

### Read database provider

The Query API selects the database provider based on the `ASPNETCORE_ENVIRONMENT` variable:

| Environment              | Provider   |
|--------------------------|------------|
| `Development`            | SQL Server |
| `Development.PostgreSql` | PostgreSQL |

The Docker Compose override defaults to PostgreSQL (`Development.PostgreSql`). When running locally you can switch by changing the environment variable and using the matching appsettings file.

## Key Patterns

- **Event Sourcing** - State is derived from a sequence of domain events stored in MongoDB. The `PostAggregate` replays events to reconstruct current state.
- **Optimistic Concurrency** - The event store checks aggregate version before persisting, throwing `ConcurrencyException` on conflicts.
- **Eventual Consistency** - The read model is updated asynchronously via Kafka. There is a brief lag between a command and its visibility on the query side.
- **Aggregate Root** - `PostAggregate` encapsulates all business rules. State changes only happen through raised events, which are applied via reflection.
- **Restore Read DB** - The `RestoreDBController` exposes a `POST /api/v1/restoredb` endpoint that replays all events from the event store to rebuild the read database from scratch. This is useful when switching database providers or recovering from a corrupted read model.
- **Query Dispatcher** - The query side uses a dispatcher pattern (`IQueryDispatcher<T>`) that routes queries to their registered handlers. Each query type (e.g. `FindAllPostsQuery`, `FindPostByIdQuery`) has a corresponding handler method in `QueryHandler`, which delegates to the `IPostRepository` for data retrieval. Handlers are registered at startup via the `QueryHandlers` service extension, keeping the controller thin and the query logic decoupled.

## Query Side Implementation

The `PostLookupController` exposes the read API and delegates all work to the query dispatcher:

```
PostLookupController  →  IQueryDispatcher<PostEntity>  →  QueryHandler  →  IPostRepository  →  PostgreSQL/SQL Server
```

| Layer | Component | Responsibility |
|-------|-----------|----------------|
| API | `PostLookupController` | Receives HTTP requests, builds query objects, returns `PostLookupResponse` DTOs |
| Queries | `FindAllPostsQuery`, `FindPostByIdQuery`, etc. | Simple query objects inheriting from `BaseQuery` |
| Handlers | `QueryHandler` (implements `IQueryHandler`) | Contains the logic for each query, calls repository methods |
| Dispatcher | `QueryDispatcher` (implements `IQueryDispatcher<PostEntity>`) | Routes query objects to the correct handler by type |
| DI Setup | `QueryHandlers` extension | Registers all query handlers with the dispatcher at startup |
