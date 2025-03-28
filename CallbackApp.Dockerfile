# Copyright 2023 MONAI Consortium
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#     http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

FROM python:3.10-alpine

RUN apk update && \
    apk upgrade && \
    apk add libcom_err=1.47.1-r1 && \
    rm -rf /var/cache/apk/*
WORKDIR /app
COPY src/TaskManager/CallbackApp/app.py ./
COPY src/TaskManager/CallbackApp/requirements.txt ./

RUN pip install -r requirements.txt

CMD ["/usr/local/bin/python3", "/app/app.py"]
