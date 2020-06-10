#!/bin/bash
docker build -t feconsole-admin --rm -f DockerFiles/Dockerfile.Admin .
docker build -t feidentityserver --rm -f DockerFiles/Dockerfile.IdentityServer .
docker build -t feconsole-api  --rm  -f DockerFiles/Dockerfile.FEConsoleAPI .
docker build -t feconsole-portal --rm -f DockerFiles/Dockerfile.FEConsolePortal .
