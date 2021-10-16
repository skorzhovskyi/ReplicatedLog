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

## Example of POST request
<img width="1359" alt="Знімок екрана 2021-10-16 о 21 41 40" src="https://user-images.githubusercontent.com/71091432/137598746-3e05d236-5dc4-425b-b7ad-8a1f31c066eb.png">
