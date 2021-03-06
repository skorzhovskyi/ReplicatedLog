# 📝 Replicated Log Secondary 2

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

```
$ SECONDARY=http://localhost:2202
$ HEADERS="Content-Type: application/json"

$ curl -XGET ${SECONDARY}/health
{"status": "ok"}

$ curl -XPOST --header ${HEADERS} ${SECONDARY}/ -d'{"messages": ["first"], "ids": [1]}'
{"status": "ok"}

$ curl -XPOST --header ${HEADERS} ${SECONDARY}/ -d'{"messages": ["third"], "ids": [3]}'
{"status": "ok"}

$ curl -XGET ${SECONDARY}/
{"status": "ok", messages": ["first"]}

$ curl -XPOST --header ${HEADERS} ${SECONDARY}/ -d'{"messages": ["second"], "ids": [2]}'
{"status": "ok"}

$ curl -XGET ${SECONDARY}/
{"status": "ok", messages": ["first", "second", "third"]}
```
