#!/bin/sh

domainName=$1

sed -i -- "s|feconsole_feadmin|$domainName|g" DockerFiles/appsettings/*appsettings.json
sed -i -- "s|feconsole_fewebapi|$domainName|g" DockerFiles/appsettings/*appsettings.json
sed -i -- "s|feconsole_feidentity|$domainName|g" DockerFiles/appsettings/*appsettings.json
sed -i -- "s|feconsole_feportal|$domainName|g"  DockerFiles/appsettings/*appsettings.json

