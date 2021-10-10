# Replicated Log Slave 2

### 🐳 Service

```
$ docker-compose -f docker-compose.yml down
$ docker-compose -f docker-compose.yml build
$ docker-compose -f docker-compose.yml up -d rep-log-slave-2
```

### 💡 Endpoints

#### 1. Add message

```
$ SLAVE=http://localhost:2202/
$ curl -XPOST --header "Content-Type: application/json" -d'{"message": "hello"}' ${SLAVE}
{"status": "ok"}
$ curl -XPOST --header "Content-Type: application/json" -d'{"message": "world"}' ${SLAVE}
{"status": "ok"}
```

#### 2. Get all messages

```
$ curl -XGET http://localhost:2202/
{
  "messages": [
    "hello",
    "world"
  ]
}
```
