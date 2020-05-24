#!/bin/bash

cp -f systemdservice/feconsoleadmin.service /etc/systemd/system/
cp -f systemdservice/feconsoleapi.service /etc/systemd/system/
cp -f systemdservice/feidentityserver.service /etc/systemd/system/
cp -f systemdservice/feconsoleportal.service /etc/systemd/system/

systemctl enable feconsoleadmin.service
systemctl enable feconsoleapi.service
systemctl enable feidentityserver.service
systemctl enable feconsoleportal.service

systemctl start feconsoleadmin.service
systemctl start feconsoleapi.service
systemctl start feidentityserver.service
systemctl start feconsoleportal.service


systemctl status feconsoleadmin.service
systemctl status feconsoleapi.service
systemctl status feidentityserver.service
systemctl status feconsoleportal.service

