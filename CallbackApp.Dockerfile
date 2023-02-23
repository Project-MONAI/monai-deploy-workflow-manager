FROM python:3.10-slim-buster

WORKDIR /app
COPY src/TaskManager/CallbackApp/app.py ./
COPY src/TaskManager/CallbackApp/requirements.txt ./

RUN pip install -r requirements.txt

CMD ["/app/app.py"]
