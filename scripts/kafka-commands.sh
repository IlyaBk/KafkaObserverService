#!/bin/bash

echo "Kafka Management Script"

case "$1" in
    "create-topic")
        docker-compose exec kafka kafka-topics --create \
            --topic user-events \
            --bootstrap-server kafka:9092 \
            --partitions 1 \
            --replication-factor 1
        ;;
    "list-topics")
        docker-compose exec kafka kafka-topics --list \
            --bootstrap-server kafka:9092
        ;;
    "produce-test")
        echo '{"userId": 123, "eventType": "click", "timestamp": "2025-04-16T12:34:56Z", "data": {"buttonId": "submit"}}' | \
        docker-compose exec -T kafka kafka-console-producer \
            --topic user-events \
            --bootstrap-server kafka:9092
        ;;
    "consume")
        docker-compose exec kafka kafka-console-consumer \
            --topic user-events \
            --bootstrap-server kafka:9092 \
            --from-beginning
        ;;
    "describe-topic")
        docker-compose exec kafka kafka-topics --describe \
            --topic user-events \
            --bootstrap-server kafka:9092
        ;;
    *)
        echo "Usage: $0 {create-topic|list-topics|produce-test|consume|describe-topic}"
        exit 1
        ;;
esac