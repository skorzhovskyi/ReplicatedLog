# 📝 Replicated Log

##  Servers

| Server  | Address | Endpoints |
| ------------- | ------------- | --- |
| **MASTER**  | `localhost:2100`  | `GET /` - Get all messages<br> `POST /` - Append message |
| **SECONDARY1**  | `localhost:2201`  |  `GET /` - Get all messages<br> `POST /` - Append message |
| **SECONDARY2**  | `localhost:2202`  | `GET /` - Get all messages<br> `POST /` - Append message |

## 🌳 Environment variables

- `BROADCASTING_TIME_OUT` - indicates duration Master should wait until Secondary receives a message (20 sec by default)
- `POST_DELAY` - indicates duration of delay for Secondary POST request

Variables can be changed in `docker-compose.yml`.

## 🐳 Running

```
# Will re-build services before running
$ docker-compose up --build

# Or use custom script
$ run.sh
```

## 🐢 Examples

### POST request to Master

```
$ curl -XPOST http://localhost:2100/ -d'{"message": "hello"}'
```

### GET requests to Master and Secondary

```
# Master
$ curl -XGET http://localhost:2100/
{"messages":["hello"]}

# First secondary
$ curl -XGET http://localhost:2201/
{"messages":["hello"]}

# Second secondary
$ curl -XGET http://localhost:2202/
{"messages":["hello"]}
```
