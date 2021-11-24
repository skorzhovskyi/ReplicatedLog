# Replicated Log Secondary 2

## 🐳 Commands

```
# Stop service if it is already running
$ docker-compose -f docker-compose.yml down

# Build new version
$ docker-compose -f docker-compose.yml build

# Run service
$ docker-compose -f docker-compose.yml up -d rep-log-secondary-2

# Check logs
$ docker logs replicatedlogsecondary2_rep-log-secondary-2_1
```

## ✨ Endpoints

### 0. Health check

```
$ SECONDARY=http://localhost:2202/
$ curl -XGET ${SECONDARY}/health
{"status": "ok"}
```

### 1. Append message

```
$ HEADERS="--header 'Content-Type: application/json'"

$ curl -XPOST ${HEADERS} -d'{"message": "hello"}' ${SECONDARY}/
{"status": "ok"}

$ curl -XPOST ${HEADERS} -d'{"message": "world"}' ${SECONDARY}/
{"status": "ok"}
```

### 2. Get all messages

```
$ curl -XGET ${SECONDARY}/
{
  "messages": [
    "hello",
    "world"
  ]
}
```
