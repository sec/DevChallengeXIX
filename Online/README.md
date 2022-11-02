# DevChallenge XIX

## Some background
1. Database used is `SQLite`
1. For normal run - it will create `trust.sdb` database file
1. For test run - it will create `tests.sdb` database file **(this was changed to use in-memory after submission was made)**
1. Database is deleted and recreated after every run
1. Person don't need topic to be created - it will still be used in traversal if requested topic list will be empty
1. It can be run without Docker - no need for any external services

## Local build/run/test

1. `dotnet build`
1. `dotnet test`
1. `dotnet run --project src\DevChallengeXIX.Web`
1. Use `https://localhost:8080` or `http://localhost:8081`

## Docker

1. `docker compose build`
1. `docker compose up`
1. Use `http://localhost:8080` to send requests to the API
1. *NOTE* Make sure to use `http` - there's no SSL certificate configured for docker image to be used

## Available endpoints and sample requests

### /api/people

```
{
    "id": "Garry",
    "topics": [
        "books",
        "magic",
        "movies"
    ]
}
```

### /api/people/Garry/trust_connections

```
{
    "Ron": 9,
    "Hermione": 9,
    "Snape": 4,
    "Voldemort": 1
}
```

### /api/messages

```
{
    "text": "Voldemort is alive!",
    "topics": [
        "magic"
    ],
    "from_person_id": "Garry",
    "min_trust_level": 9
}
```

### /api/path

```
{
    "text": "Voldemort is alive!",
    "topics": [
        "books"
    ],
    "from_person_id": "Garry",
    "min_trust_level": 9
}
```