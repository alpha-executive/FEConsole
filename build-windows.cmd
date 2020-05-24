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
xcopy /Y /Q "windows-scripts\startfeconsoleadmin.bat" "fepublish/feconsoleadmin"
xcopy /Y /Q "windows-scripts\startfeconsoleapi.bat" "fepublish/feconsoleapi"
xcopy /Y /Q "windows-scripts\startfeconsoleportal.bat" "fepublish/feconsoleportal"
xcopy /Y /Q "windows-scripts\startfeidentityserver.bat" "fepublish/feidentityserver"
xcopy /Y /Q "windows-scripts\startall.bat" "fepublish"
xcopy /Y /Q "windows-scripts\quickstart.bat" "fepublish"
