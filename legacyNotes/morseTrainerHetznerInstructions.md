## Namecheap
Type: A
Host: morse
Value: 49.12.76.128

## NGINX
```nginx
server {
  server_name morse.portfolio-projects.space;

  location /socket.io/ {
    proxy_pass http://localhost:3014/socket.io/;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "Upgrade";
    proxy_set_header Host $host;
  }

  location / {
    proxy_pass http://localhost:3014;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
  }

  listen 80;
}
```

## Hetzner
ssh root@49.12.76.128
```bash
cd /var/www
mkdir morseTrainer
cd morseTrainer
git clone git@github.com:alkisax/morseTrainer.git .
cd backend-node
npm install
nano .env
npm run build
pm2 start build/src/server.js --name morse-backend
pm2 save
pm2 list
nano /etc/nginx/sites-available/morse.portfolio-projects.space

cat /etc/nginx/sites-available/morse.portfolio-projects.space

ln -s /etc/nginx/sites-available/morse.portfolio-projects.space /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx

certbot --nginx -d morse.portfolio-projects.space
systemctl reload nginx
```

## notes
ssh root@49.12.76.128
```bash
cd /var/www/morseTrainer/backend-node \
&& git pull origin main \
&& npm install \
&& npm run build \
&& pm2 restart morse-backend --update-env \
&& nginx -t \
&& systemctl reload nginx \
&& curl https://morse.portfolio-projects.space/health \
&& echo "\n✓ morse deploy OK"
```
pm2 logs morse-backend --lines 50
pm2 flush morse-backend