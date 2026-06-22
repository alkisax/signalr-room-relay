## Namecheap :

Type: A
Host: signalr-room-relay
Value: 49.12.76.128
TTL: Automatic

ssh root@49.12.76.128

## nginx
```nginx
server {
    server_name signalr-room-relay.portfolio-projects.space;

    location / {
        proxy_pass http://localhost:3017;

        proxy_http_version 1.1;

        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "Upgrade";

        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## bash
```bash
cd /var/www
mkdir signalr-room-relay
cd signalr-room-relay
git clone git@github.com:alkisax/signalr-room-relay.git .
cd backend-dotnet

# -- test
cd /var/www/signalr-room-relay/backend-dotnet
dotnet publish -c Release -o out

cd /var/www/signalr-room-relay/backend-dotnet/out
dotnet backend-dotnet.dll

# --pm2
cd /var/www/signalr-room-relay/backend-dotnet/out

pm2 start "dotnet backend-dotnet.dll" --name signalr-room-relay
pm2 logs signalr-room-relay --lines 30
curl http://localhost:3017/health

# --nginx
nano /etc/nginx/sites-available/signalr-room-relay
cat /etc/nginx/sites-available/signalr-room-relay

ln -s /etc/nginx/sites-available/signalr-room-relay /etc/nginx/sites-enabled/

nginx -t
systemctl reload nginx

curl http://signalr-room-relay.portfolio-projects.space/health

certbot --nginx -d signalr-room-relay.portfolio-projects.space
curl https://signalr-room-relay.portfolio-projects.space/health
```

## oneLine deploy
ssh root@49.12.76.128

```bash
cd /var/www/signalr-room-relay && git pull origin main && cd backend-dotnet && rm -rf out bin obj && dotnet publish -c Release -o out && pm2 restart signalr-room-relay && curl https://signalr-room-relay.portfolio-projects.space/health

```
pm2 logs signalr-room-relay --lines 50
pm2 flush signalr-room-relay