#!/bin/sh
packagename=$1
currdir=`pwd`
installdir=$currdir/feconsole

mkdir -p feconsole
tar -xzvf $packagename -C feconsole
ln -s $installdir /var/www/feconsole

