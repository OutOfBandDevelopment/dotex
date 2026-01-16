# Redis Caching

## Summary

One of the caching store options for OoBDev Caching Framework is Redis

## Configurations

| Key                                     | Default      | Note                                   |
| --------------------------------------- | ------------ | -------------------------------------- |
| Redis:ConnectionMultiplexer:Config      |              | Redis Multiplexer configuration string |

## Setup Docker

https://hub.docker.com/_/redis/

```shell
docker pull redis
docker run --name oobdev-redis -d -p 6379:6379 redis
```

## Redis-CLI

You can install redis-cli with https://chocolatey.org/
http://ppanyukov.github.io/2015/05/21/how-to-run-redis-in-docker-on-windows.html

```shell
choco install redis-64 --version=3.0.503
```