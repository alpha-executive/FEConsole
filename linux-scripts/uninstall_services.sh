#!/bin/bash
./stop_service.sh

systemctl disable feconsoleadmin.service
systemctl disable feconsoleapi.service
systemctl disable feidentityserver.service
systemctl disable feconsoleportal.service

rm -f /etc/systemd/system/feconsoleadmin.service
rm -f /etc/systemd/system/feconsoleapi.service
rm -f /etc/systemd/system/feidentityserver.service
rm -f /etc/systemd/system/feconsoleportal.service

