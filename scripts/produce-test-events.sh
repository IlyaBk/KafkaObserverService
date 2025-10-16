#!/bin/bash

# Ожидаем пока запустится Kafka
echo "Waiting for Kafka to be ready..."
sleep 30

# Создаем топик
docker-compose exec kafka kafka-topics --create --topic user-events --bootstrap-server kafka:9092 --partitions 1 --replication-factor 1

# Создаём тестовые события
echo "Producing test events to Kafka..."

for i in {1..10}; do
    USER_ID=$(( ( RANDOM % 5 ) + 1 ))
    EVENT_TYPES=("click" "hover" "submit" "view" "scroll")
    EVENT_TYPE=${EVENT_TYPES[$RANDOM % ${#EVENT_TYPES[@]}]}
    
    EVENT_JSON=$(cat <<EOF
{
  "userId": $USER_ID,
  "eventType": "$EVENT_TYPE",
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "data": {
    "buttonId": "button-$(( RANDOM % 3 ))"
  }
}
EOF
)

    echo $EVENT_JSON | docker-compose exec -T kafka kafka-console-producer --topic user-events --bootstrap-server kafka:9092
    echo "Produced event for user $USER_ID, type: $EVENT_TYPE"
    sleep 1
done

echo "Test events produced successfully!"