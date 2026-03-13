docker run -d --name redis -p 6379:6379 redis:8.6
docker exec -it redis redis-cli

docker compose -f .\observability\lgtm\docker-compose.yml up -d

