
IF "%APP_PROJECT%"=="" SET APP_PROJECT=eliassen-libs-dev

docker compose --project-name %APP_PROJECT% stop