set ASPNETCORE_ENVIRONMENT=HttpProd
pushd .
echo "starting feconsle admin..."
cd feconsoleadmin
start /B startfeconsoleadmin.bat
popd

pushd .
echo "starting feconsle api..."
cd feconsoleapi
start /B startfeconsoleapi.bat
popd

pushd .
echo "starting feconsle portal..."
cd feconsoleportal
start /B startfeconsoleportal.bat
popd

pushd .
echo "starting feconsle identity server..."
cd feidentityserver
start /B startfeidentityserver.bat
popd

start https://localhost:8090