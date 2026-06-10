.PHONY: apphost watch test docker-up docker-down docker-down-volumes generate-api

apphost:
	cd backend && dotnet run --project ./src/AppHost

watch:
	cd backend && dotnet watch --project ./src/AppHost run

test:
	cd backend && dotnet test

docker-up:
	docker compose up --build

docker-down:
	docker compose down

docker-down-volumes:
	docker compose down -v

generate-api:
	cd frontend && npm run generate:api-spec