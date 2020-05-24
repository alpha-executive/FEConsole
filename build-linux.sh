#!/bin/bash

echo "pull code from github..."
git pull
echo "pull code done"


function buildproject()
{
  platform=$1
  echo "start build the $platform binaries..."
  dotnet clean
  dotnet publish -r $platform -c Release
  echo "linux-x64 done"

  return 0
}

function copycert()
{
  platformpath=$1
  echo "deploy certificate key to $platformpath..."
  cp -f ./localhost.pfx ./FE.Creator.FEConsoleAPI/bin/Release/netcoreapp3.1/$platformpath
  cp -f ./localhost.pfx ./FE.Creator.IdentityServer/bin/Release/netcoreapp3.1/$platformpath
  cp -f ./localhost.pfx ./FE.Creator.Admin/bin/Release/netcoreapp3.1/$platformpath
  cp -f ./localhost.pfx ./FE.Creator.FEConsolePortal/bin/Release/netcoreapp3.1/$platformpath
  cp -f ./localhost.pfx ./FE.Creator.PCenter/bin/Release/netcoreapp3.1/$platformpath

  echo "certificate deployed"
  return 0
}

function cleanpublishdir()
{
  local platform=$1
  mkdir -p fepublish/$platform
  rm -rf fepublish/$platform/*

  mkdir -p fepublish/$platform/feconsoleapi
  mkdir -p fepublish/$platform/feidentityserver
  mkdir -p fepublish/$platform/feconsoleadmin
  mkdir -p fepublish/$platform/feconsoleportal
  return 0
}

function deploystartupscript(){
  local platform=$1
  cp -f linux-scripts/quickstart.sh fepublish/$platform
  cp -f linux-scripts/install_as_service.sh  fepublish/$platform
  cp -f linux-scripts/stop_service.sh fepublish/$platform
  cp -f linux-scripts/uninstall_services.sh fepublish/$platform
  cp -rf linux-scripts/systemdservice fepublish/$platform
}

function generatepackage()
{
  echo "creating release package"
  platformpath=$1
  platform=$2

  cp -rf ./FE.Creator.FEConsoleAPI/bin/Release/netcoreapp3.1/$platformpath fepublish/$platform/feconsoleapi
  cp -rf ./FE.Creator.IdentityServer/bin/Release/netcoreapp3.1/$platformpath fepublish/$platform/feidentityserver
  cp -rf ./FE.Creator.Admin/bin/Release/netcoreapp3.1/$platformpath fepublish/$platform/feconsoleadmin
  cp -rf ./FE.Creator.FEConsolePortal/bin/Release/netcoreapp3.1/$platformpath fepublish/$platform/feconsoleportal

  return 0
}

function finalizepackages()
{
   platform=$1
   tar -czvf fepublish/feconsole_$platform.tar.gz -C fepublish/$platform .
}

function deployinstallscript()
{
  cp -f linux-scripts/install.sh fepublish/
}

buildproject "linux-x64"
cleanpublishdir "linux-x64"
copycert "linux-x64/publish/"

echo "deploy database"
cp -f ./FE.Creator.IdentityServer/fetechhub_identityserver.db ./FE.Creator.IdentityServer/bin/Release/netcoreapp3.1/linux-x64/publish
echo "database done"

generatepackage "linux-x64/publish/*" "linux-x64"
deploystartupscript "linux-x64"
finalizepackages "linux-x64"

buildproject "linux-musl-x64"
cleanpublishdir "linux-musl-x64"
copycert "linux-musl-x64/"

echo "deploy database"
cp -f ./FE.Creator.IdentityServer/fetechhub_identityserver.db ./FE.Creator.IdentityServer/bin/Release/netcoreapp3.1/linux-musl-x64/publish
echo "database done"
generatepackage "linux-musl-x64/publish/*" "linux-musl-x64"
deploystartupscript "linux-musl-x64"
finalizepackages "linux-musl-x64"

buildproject "linux-arm64"
cleanpublishdir "linux-arm64"
copycert "linux-arm64/publish/"

echo "deploy database"
cp -f ./FE.Creator.IdentityServer/fetechhub_identityserver.db ./FE.Creator.IdentityServer/bin/Release/netcoreapp3.1/linux-arm64/publish
echo "database done"
generatepackage "linux-arm64/publish/*" "linux-arm64"
deploystartupscript "linux-arm64"
finalizepackages "linux-arm64"

deployinstallscript
