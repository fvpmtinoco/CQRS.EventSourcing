# CQRS.EventSourcing

A proof-of-concept implementing **CQRS** (Command Query Responsibility Segregation) with **Event Sourcing** using .NET 10, Kafka, MongoDB and SQL Server.

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
               │   MongoDB   │     │  SQL Server │
               │ Event Store │     │ Read Model  │
               └──────┬──────┘     └──────▲──────┘
                      │                   │
                      │  ┌────────────┐   │
                      └──► Apache     ├───┘
                         │   Kafka    │
                         └────────────┘
```

**Write side** receives commands, validates them through the `PostAggregate`, persists domain events to MongoDB, and publishes them to Kafka.

**Read side** consumes events from Kafka and projects them into a denormalized SQL Server model optimized for queries.

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
| Read Database   | SQL Server 2022 Express             |
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

This starts Kafka, MongoDB, SQL Server, the Command API and the Query API. The Query API automatically creates the SQL Server database on first run.

### Infrastructure only

To run just the infrastructure (Kafka, MongoDB, SQL Server) and develop locally:

```bash
docker compose -f docker-compose.yml up -d
```

## Key Patterns

- **Event Sourcing** - State is derived from a sequence of domain events stored in MongoDB. The `PostAggregate` replays events to reconstruct current state.
- **Optimistic Concurrency** - The event store checks aggregate version before persisting, throwing `ConcurrencyException` on conflicts.
- **Eventual Consistency** - The read model is updated asynchronously via Kafka. There is a brief lag between a command and its visibility on the query side.
- **Aggregate Root** - `PostAggregate` encapsulates all business rules. State changes only happen through raised events, which are applied via reflection.
