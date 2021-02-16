#!/bin/bash

function builddockerimages()
{
   docker build -t feconsole-admin --rm -f DockerFiles/Dockerfile.Admin .
   docker build -t feidentityserver --rm -f DockerFiles/Dockerfile.IdentityServer .
   docker build -t feconsole-api  --rm  -f DockerFiles/Dockerfile.FEConsoleAPI .
   docker build -t feconsole-portal --rm -f DockerFiles/Dockerfile.FEConsolePortal .
}
function cleanpublishdir()
{
   mkdir -p fepublish/docker-swarm
   rm -rf fepublish/docker-swarm/*
   mkdir -p fepublish/docker-swarm/DockerFiles
}
function deploydockerfiles()
{
   cp -rf DockerFiles/appsettings fepublish/docker-swarm/DockerFiles
   cp docker-compose.yml fepublish/docker-swarm/docker-compose.yml
   cp docker-scripts/deploystack.sh fepublish/docker-swarm/deploystack.sh
   cp docker-scripts/undeploystack.sh fepublish/docker-swarm/undeploystack.sh
   cp docker-scripts/setupconfig.sh fepublish/docker-swarm/setupconfig.sh
}

builddockerimages
cleanpublishdir
deploydockerfiles
