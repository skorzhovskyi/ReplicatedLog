# Replicated Log

## End points

```
Master - localhost:2100
Secondary1 - localhost:2201
Secondary2 - localhost:2202 
```

## Docker-compose variables

```
BROADCASTING_TIME_OUT - indicates duration Master should wait until secondary receives a message (20 sec by default)
POST_DELAY - indicates duration of delay for Secondary1 post request (10 sec by default)
```

## Runing

```
run.sh
```
