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

## 🧪 Testing

### Simple case

Stop one secondary and send messages with different write concern

```
$ docker-compose up --build
$ docker pause replicatedlog_rep-log-secondary-2_1
$ curl -XPOST http://localhost:2100/ -d'{"message": "m1", "w": 1}' # Ok
$ curl -XPOST http://localhost:2100/ -d'{"message": "m2", "w": 2}' # Ok
$ curl -XPOST http://localhost:2100/ -d'{"message": "m3", "w": 3}' # Hangs
$ curl -XPOST http://localhost:2100/ -d'{"message": "m4", "w": 1}' # Ok
$ docker unpause replicatedlog_rep-log-secondary-2_1
```

All messages must arrive after secondary was resurrected.

### Testing ordering

Set `ERROR_BEFORE_EVEN_MESSAGE = true` for secondary and send at least 3 messages. Even messages (messages with even id)
will never be added into the queue and third message will never be displayed since second did not arrive.

### Testing deduplication

Set `ERROR_AFTER_EVEN_MESSAGE = true` for secondary and send at least 2 messages. Error will be generated after second
message was added into the queue and master will continue with retries. Retried messages will not be added on secondary
since message already exists.
