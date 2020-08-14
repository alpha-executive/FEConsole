#! /bin/sh
docker service rm feconsole_feadmin
docker service rm feconsole_feidentity
docker service rm feconsole_feportal
docker service rm feconsole_fewebapi

docker volume rm appdata
docker volume rm dbdata
docker network rm feconsole-vnet

