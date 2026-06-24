docker run -d --name redis -p 6379:6379 redis:8.6
docker exec -it redis redis-cli

docker compose -f .\observability\lgtm\docker-compose.yml up -d

docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4-management