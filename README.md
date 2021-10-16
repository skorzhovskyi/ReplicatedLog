# Replicated Log

## End points

```
Master - localhost:2100
Slave1 - localhost:2201
Slave2 - localhost:2202 
```

## Docker-compose variables

```
BROADCASTING_TIME_OUT - tells how long Master waits until a slave received a message (20 sec by default)
POST_DELAY - Slave1 post request delay (10 sec by default)
```

## Runing

```
run.sh
```