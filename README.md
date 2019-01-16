# SomfyWebApi
Asp.net Core WebApi for controlling a Raspberry to close and open Somfy blinds

# Build and Publish
`dotnet publish -c Release -r linux-arm`

copy bin\Release\netcoreapp2.1\linux-arm\publish to raspberry pi `home/pi/SomPiWebApi`

# Setup Raspberry Pi with Nginx and .net core:
`sudo apt-get install curl libunwind8 gettext apt-transport-https`

`chmod 755 ./SomPiWebApi`

`sudo apt-get install nginx`

edit: nginx config 
`sudo nano /etc/nginx/sites-available/default`

```
location / {
     proxy_pass http://localhost:5000/;
     proxy_http_version 1.1;
     proxy_set_header Connection keep-alive;
      proxy_set_header X-Forwarded-For    $proxy_add_x_forwarded_for;
      proxy_set_header X-Forwarded-Host   $http_host;
      proxy_set_header X-Forwarded-Proto  http;
}
```

restart nginx
`sudo nginx -s reload`

# Add Autostart of webapp
create SomPiWebApi.service file in the /lib/systemd/system/

```
[Unit]
Description=Somfy WebApi
After=nginx.service
 
[Service]
Type=simple
WorkingDirectory=/home/pi/SomPiWebApi
ExecStart=/home/pi/SomPiWebApi/SomPiWebApi
Restart=always
```

`sudo systemctl enable SomPiWebApi`

`sudo systemctl start SomPiWebApi`


make calls to web api:
POST http://raspberrypi/api/blinds/livingRoomDoor?action=down

