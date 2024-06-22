# Example - Keycloak

## Summary

## Users 

User Name | Default Password | Roles 
--------- | ---------------- | ------------------------
admin     | admin            | system administration
adele     | Adele123!        | application admin


## Connect

```bash
docker exec -it keycloak /bin/bash
```

## Export

```bash
/opt/keycloak/bin/kc.sh export --dir /opt/keycloak/data/import --users realm_file --realm local-dev
```

```cmd
docker exec keycloak /opt/keycloak/bin/kc.sh export --dir /opt/keycloak/data/import --users realm_file --realm local-dev
```

## Backup

```bash
export DATA_PATH=/tmp/keycloak-data/local-dev
export KCADMIN_CONFIG=$DATA_PATH/kcadm.config
cd /opt/keycloak/bin

# ./kc.sh export --dir $DATA_PATH --realm local-dev --users different_files
 ./kc.sh export --dir $DATA_PATH --users realm_file --realm local-dev
```

## Import

```bash
export DATA_PATH=/tmp/keycloak-data/local-dev
export KCADMIN_CONFIG=$DATA_PATH/kcadm.config
cd /opt/keycloak/bin

./kc.sh import --file /tmp/keycloak-data/local-dev/local-dev-realm.json
```

## Restore

```bash
export DATA_PATH=/tmp/keycloak-data
export KCADMIN_CONFIG=$DATA_PATH/kcadm.config
cd /opt/keycloak/bin
./kcadm.sh config credentials --server http://localhost:8080 --realm master --user admin --password admin --config $KCADMIN_CONFIG

./kcadm.sh create realms --file $DATA_PATH/realm-export.json --config $KCADMIN_CONFIG

./kcadm.sh create clients --target-realm local-dev --file $DATA_PATH/client-dotnet-webapi.json --config $KCADMIN_CONFIG
# ./kcadm.sh create clients --target-realm local-dev --file $DATA_PATH/client-node-express.json --config $KCADMIN_CONFIG
# ./kcadm.sh create clients --target-realm local-dev --file $DATA_PATH/client-java-springboot.json --config $KCADMIN_CONFIG

./kcadm.sh create users --target-realm local-dev --set username=adele --set enabled=true --set firstName=Adele --set lastName=Admin --set email=adele@fake.io --config $KCADMIN_CONFIG
./kcadm.sh set-password --target-realm local-dev --username adele --new-password Adele123! --config $KCADMIN_CONFIG
```


## Token Test Queries

* [JWT.IO](http://localhost:8081/realms/local-dev/protocol/openid-connect/auth?response_type=token&client_id=dotnet-webapi&redirect_uri=https%3A%2F%2Fjwt.io%2F)
* [JWT.MS](http://localhost:8081/realms/local-dev/protocol/openid-connect/auth?response_type=token&client_id=dotnet-webapi&redirect_uri=https%3A%2F%2Fjwt.ms%2F)
* [Logout](http://localhost:8081/realms/local-dev/protocol/openid-connect/logout?client_id=dotnet-webapi)

## Notes and references

* https://stackoverflow.com/questions/53550321/keycloak-gatekeeper-aud-claim-and-client-id-do-not-match
  * you need to remove the audience resolve in Client Scopes -> roles -> Mappers 
* https://simonscholz.github.io/tutorials/keycloak-realm-export-import
* https://github.com/bitnami/charts/tree/main/bitnami/keycloak
* https://docs.bitnami.com/kubernetes/apps/keycloak/get-started/configure-authentication/
* https://docs.bitnami.com/tutorials/integrate-keycloak-authentication-kubernetes/
* https://gist.github.com/peteristhegreat/1f4a9bdb457f5b76be22afa74f98a7f2
* https://www.keycloak.org/docs/latest/server_development/
* https://www.keycloak.org/server/containers
* https://bitnami.com/stack/keycloak
* https://davidtruxall.com/keycloak-for-local-development/
* https://stackoverflow.com/questions/75649136/unable-to-bring-up-the-keycloak-instance-in-local-using-bitnami-image
* https://github.com/bitnami/containers/tree/main/bitnami/keycloak
* https://stackoverflow.com/questions/71376841/keycloak-setup-on-kubernetes
* https://code.kx.com/insights/1.4/enterprise/security/backup-and-restore.html
* https://nikiforovall.github.io/aspnetcore/dotnet/2022/08/24/dotnet-keycloak-auth.html
* https://stackoverflow.com/questions/50685882/setting-up-realms-in-keycloak-during-kubernetes-helm-install
* https://docs.bitnami.com/tutorials/integrate-keycloak-authentication-kubernetes/#step-2-create-a-keycloak-realm-and-user
* https://stackoverflow.com/questions/63640401/how-to-configure-keycloak-helm-chart
* https://www.keycloak.org/server/importExport