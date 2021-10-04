FROM python:3.9-slim

WORKDIR /opt/app

COPY requirements.txt /opt/app/

RUN pip install -r requirements.txt

COPY service.py /opt/app/

EXPOSE 5000