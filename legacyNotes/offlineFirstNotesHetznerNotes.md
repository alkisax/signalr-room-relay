## nameCheap
Domain List → Manage → Advanced DNS
Type: A
Host: offline-first
Value: 49.12.76.128
TTL: Automatic

## NGINX
```nginx
server {
  server_name offline-first.portfolio-projects.space;

  location / {
    proxy_pass http://localhost:3014;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection 'upgrade';
    proxy_set_header Host $host;
  }

  listen 80;
}
```

## dotnet hetzner init
ssh root@49.12.76.128

cd /var/www
mkdir offline-first
cd offline-first

git clone git@github.com:alkisax/offline-first-notes-native-app.git .
git checkout main

⚠️ κάνω Install το dotnet 10 γιατί είναι πρωτη φορα στον server
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb

apt update
apt install -y dotnet-sdk-10.0
dotnet --version

cd /var/www/offline-first/backend-csharp
dotnet publish -c Release -o out

pm2 start "dotnet backend-csharp.dll" --name offline-backend
pm2 save
pm2 startup

nano /etc/nginx/sites-available/offline-first.portfolio-projects.space
cat /etc/nginx/sites-available/offline-first.portfolio-projects.space

ln -s /etc/nginx/sites-available/offline-first.portfolio-projects.space /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx
curl http://offline-first.portfolio-projects.space/health

certbot --version
certbot --nginx -d offline-first.portfolio-projects.space

curl https://offline-first.portfolio-projects.space/health
curl https://offline-first.portfolio-projects.space/api/ping

pm2 logs offline-backend --lines 50
pm2 flush offline-backend

ssh root@49.12.76.128
```bash
cd /var/www/offline-first \
&& git pull origin main \
&& cd backend-csharp \
&& dotnet publish -c Release -o out \
&& pm2 restart offline-backend \
&& sleep 2 \
&& curl -f http://localhost:3014/health \
&& nginx -t && systemctl reload nginx \
&& echo "✓ offline-first deploy OK"
```


🤖 Android app:
https://expo.dev/artifacts/eas/cJLQxgzNoCjQMVHqnLnCf5.aab