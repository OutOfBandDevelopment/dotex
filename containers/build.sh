#!/bin/bash

if [ -z "$APP_PROJECT" ]; then
  APP_PROJECT="eliassen-libs-dev"
fi

docker compose --project-name "$APP_PROJECT" --file docker-compose-cpu.yml build
