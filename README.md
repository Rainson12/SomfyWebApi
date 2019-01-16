# SomfyWebApi
Asp.net Core WebApi for controlling a Raspberry to close and open Somfy blinds

# Build and Publish
`dotnet publish -c Release -r linux-arm`

copy bin\Release\netcoreapp2.1\linux-arm\publish to raspberry pi

# Setup Raspberry Pi with Nginx and .net core:
`sudo apt-get install curl libunwind8 gettext apt-transport-https`

`chmod 755 ./SomPiWebApi`

sudo apt-get install nginx

edit: 
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

`sudo nginx -s reload`

# Add Autostart of webapp
create SomPiWebApi.service file in the /lib/systemd/system/

```
[Unit]
Description=Somfy WebApi
After=nginx.service
 
[Service]
Type=simple
WorkingDirectory=/home/pi/apps/MyWebApp
ExecStart=/home/pi/apps/MyWebApp/MyWebApp
Restart=always
```

`sudo systemctl enable SomPiWebApi`

`sudo systemctl start SomPiWebApi`


make calls to web api:
POST http://raspberrypi/api/blinds/livingRoomDoor?action=down

