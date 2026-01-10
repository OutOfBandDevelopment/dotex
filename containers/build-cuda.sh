#!/bin/bash

if [ -z "$APP_PROJECT" ]; then
  APP_PROJECT="libs-dev"
fi

docker compose --project-name "$APP_PROJECT" --file docker-compose-cuda.yml build
