#!/bin/sh
packagename=$1
currdir=`pwd`
installdir=$currdir/feconsole

mkdir -p feconsole
tar -xzvf $packagename -C feconsole

mkdir -p /var/www
ln -s $installdir -t /var/www

sudo cp localhost.crt /usr/local/share/ca-certificates
sudo update-ca-certificates
