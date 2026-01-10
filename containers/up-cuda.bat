
IF "%APP_PROJECT%"=="" SET APP_PROJECT=libs-dev

docker compose --project-name %APP_PROJECT% --file docker-compose-cuda.yml up --detach