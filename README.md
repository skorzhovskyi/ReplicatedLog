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

## Examples

### POST request to Master

```
$ curl -XPOST http://localhost:2100/ -d'{"message": "hello"}'
```

<img width="1359" alt="Знімок екрана 2021-10-16 о 21 41 40" src="https://user-images.githubusercontent.com/71091432/137598746-3e05d236-5dc4-425b-b7ad-8a1f31c066eb.png">

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

<img width="1359" alt="Знімок екрана 2021-10-16 о 21 42 45" src="https://user-images.githubusercontent.com/71091432/137598785-8bf0c396-21e0-46e8-b9f2-bd67e363354b.png">

### Acknowledgements in Terminal 

<img width="797" alt="Знімок екрана 2021-10-16 о 21 47 29" src="https://user-images.githubusercontent.com/71091432/137598937-45f79e43-9626-4455-af92-351abff9cd81.png">
