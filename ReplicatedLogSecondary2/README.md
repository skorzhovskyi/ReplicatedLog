# Replicated Log Secondary 2

## 🐳 Service

```
$ docker-compose -f docker-compose.yml down
$ docker-compose -f docker-compose.yml build
$ docker-compose -f docker-compose.yml up -d rep-log-secondary-2
$ docker logs replicatedlogsecondary2_rep-log-secondary-2_1
```

## 💡 Endpoints

### 1. Append message

```
$ SECONDARY=http://localhost:2202/
$ curl -XPOST --header "Content-Type: application/json" -d'{"message": "hello"}' ${SECONDARY}
{"status": "ok"}
$ curl -XPOST --header "Content-Type: application/json" -d'{"message": "world"}' ${SECONDARY}
{"status": "ok"}
```

### 2. Get all messages

```
$ curl -XGET http://localhost:2202/
{
  "messages": [
    "hello",
    "world"
  ]
}
```
