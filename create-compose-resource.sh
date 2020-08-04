#! /bin/sh
docker volume create appdata
docker volume create dbdata

docker network create -d overlay feconsole-vnet

