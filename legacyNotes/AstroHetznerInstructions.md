- Namecheap
Type: A
Host: astro
Value: 49.12.76.128

```nginx
server {
  server_name astro.portfolio-projects.space;

  location /api/ {
    proxy_pass http://localhost:3013;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection 'upgrade';
    proxy_set_header Host $host;
  }

  location / {
    root /var/www/astroWrap/frontend/dist;
    index index.html;
    try_files $uri $uri/ /index.html;
  }

  listen 80;
}
```

ssh root@49.12.76.128

cd /var/www
mkdir astroWrap
cd astroWrap

git clone git@github.com:alkisax/astroWrap.git .
git checkout main
git branch
ls

cd backend
npm install
nano .env
npm run build

cd ../frontend
npm install
nano .env
npm run build

cd /var/www/astroWrap/backend
pm2 start build/server.js --name astro-backend
pm2 save
pm2 list
curl http://localhost:3013/api/ping

nano /etc/nginx/sites-available/astro.portfolio-projects.space

ln -s /etc/nginx/sites-available/astro.portfolio-projects.space /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx

certbot --nginx -d astro.portfolio-projects.space
systemctl reload nginx

curl https://astro.portfolio-projects.space/api/ping

pm2 flush astro-backend
pm2 logs astro-backend --lines 50


ssh root@49.12.76.128
```bash
cd /var/www/astroWrap \
&& git pull origin main \
&& cd frontend && npm install --legacy-peer-deps && npm run build \
&& cd ../backend && npm install && npm run build \
&& pm2 restart astro-backend --update-env \
&& nginx -t && systemctl reload nginx \
&& echo "✓ astro deploy OK"
```