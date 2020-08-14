#!/bin/bash

function createvolumes()
{
  docker volume create appdata
  docker volume create dbdata
}

function createvnet()
{
  docker network create -d overlay feconsole-vnet
}

function deploystack()
{
  docker stack deploy -c docker-compose.yml feconsole
}

function listservices()
{
  docker service list
}

function setupconfigs
{
  ./setupconfig.sh "$1"
}

if [ ! -n "$1" ]; then
  echo "===================================ERROR============================="
  echo "= please provide the host domain name (ip)                          ="
  echo "= example: 192.168.3.10 or mylaptop                                 ="
  echo "====================================================================="
else
  echo "creating resources..."
  createvolumes
  createvnet 
  
  echo "setup configs..."
  setupconfigs "$1"

  echo "deploying stack..."
  deploystack
  listservices
fi


