@echo off
echo "start build on windows"
dotnet clean
dotnet publish -c Release

echo "packing application..."
rmdir /S /Q fepublish

mkdir fepublish\feconsoleadmin
xcopy /Y /S /Q "FE.Creator.Admin/bin/Release/netcoreapp3.1/publish" "fepublish/feconsoleadmin"

mkdir fepublish\feconsoleapi
xcopy /Y /S /Q "FE.Creator.FEConsoleAPI/bin/Release/netcoreapp3.1/publish" "fepublish/feconsoleapi"

mkdir fepublish\feconsoleportal
xcopy /Y /S /Q "FE.Creator.FEConsolePortal/bin/Release/netcoreapp3.1/publish" "fepublish/feconsoleportal"

mkdir fepublish\feidentityserver
xcopy /Y /S /Q "FE.Creator.IdentityServer/bin/Release/netcoreapp3.1/publish" "fepublish/feidentityserver"

echo "deploy certificate key"
xcopy /Y /Q "localhost.pfx" "fepublish/feconsoleadmin"
xcopy /Y /Q "localhost.pfx" "fepublish/feconsoleapi"
xcopy /Y /Q "localhost.pfx" "fepublish/feconsoleportal"
xcopy /Y /Q "localhost.pfx" "fepublish/feidentityserver"

echo "deploy startup scripts"
xcopy /Y /Q "startscripts\startfeconsoleadmin.bat" "fepublish/feconsoleadmin"
xcopy /Y /Q "startscripts\startfeconsoleapi.bat" "fepublish/feconsoleapi"
xcopy /Y /Q "startscripts\startfeconsoleportal.bat" "fepublish/feconsoleportal"
xcopy /Y /Q "startscripts\startfeidentityserver.bat" "fepublish/feidentityserver"
xcopy /Y /Q "startscripts\startall.bat" "fepublish"
xcopy /Y /Q "startscripts\quickstart.bat" "fepublish"