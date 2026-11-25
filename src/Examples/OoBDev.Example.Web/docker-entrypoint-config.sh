#!/bin/sh
# Writes /config.json from the container environment so one image runs against any identity provider.
cat > /usr/share/nginx/html/config.json <<JSON
{ "oidcAuthority": "${OIDC_AUTHORITY:-}", "oidcClientId": "${OIDC_CLIENT_ID:-oobdev-example-web}" }
JSON
