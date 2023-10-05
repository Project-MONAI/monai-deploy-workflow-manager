#!/usr/bin/env python

# Copyright 2023 MONAI Consortium
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import argparse
import pika
import uuid

from time import perf_counter as pc

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", type=str, required=True)
    parser.add_argument("--username", type=str, required=True)
    parser.add_argument("--password", type=str, required=True)
    parser.add_argument("--vhost", type=str, required=True)
    parser.add_argument("--exchange", type=str, required=True)
    parser.add_argument("--topic", type=str, required=True)
    parser.add_argument("--correlationId", type=str, required=True)
    parser.add_argument("--message", type=str, required=True)
    parser.add_argument("--secure", default=False, type=bool)
    args = parser.parse_args()

    print(f"[Correlation ID={args.correlationId}] Sending message to {args.host} at exchange={args.exchange}, topic={args.topic}...")
    print(f"[Correlation ID={args.correlationId}] Message={args.message}...")

    credentials = pika.PlainCredentials(args.username, args.password)
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host=args.host, virtual_host=args.vhost, credentials=credentials))
    channel = connection.channel()

    properties = pika.BasicProperties(
        content_type="application/json",
        message_id=str(uuid.uuid4()),
        app_id="Task Manager Callback",
        correlation_id=args.correlationId,
        delivery_mode=2,
        type=args.topic)

    channel.basic_publish(exchange=args.exchange, routing_key=args.topic, body=args.message, properties=properties)
    connection.close()
    print('Message sent.')

if __name__ == "__main__":
    t0 = pc()
    try:
        main()
    except Exception as e:
        print(e)
    print(f"Duration: {pc()-t0}")
