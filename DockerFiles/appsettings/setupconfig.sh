#!/bin/sh

adminUrl=$1
apiUrl=$2
identityUrl=$3
portalUrl=$4

sed -i -- "s|http:\/\/feconsole_feadmin:8090|$adminUrl|g" *appsettings.json
sed -i -- "s|http:\/\/feconsole_fewebapi:8091|$apiUrl|g" *appsettings.json
sed -i -- "s|http:\/\/feconsole_feidentity:8092|$identityUrl|g" *appsettings.json
sed -i -- "s|http:\/\/feconsole_feportal:8093|$portalUrl|g" *appsettings.json

