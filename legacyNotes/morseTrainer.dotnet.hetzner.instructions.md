## namecheap
Type: A
Host: morse-dotnet
Value: 49.12.76.128
TTL: Automatic

## nginx
```nginx
server {
    server_name morse-dotnet.portfolio-projects.space;

    location / {
        proxy_pass http://localhost:3016;

        proxy_http_version 1.1;

        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "Upgrade";

        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## dotnet install
στο server Μου έχω ήδη dotnet οποτε δεν θα τα κάνω αυτά. αν δεν είχα όμως
```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update
apt install -y dotnet-sdk-10.0
dotnet --version
```

## Hetzner
```bash
ssh root@49.12.76.128
cd /var/www
mkdir morseTrainer-dotnet
cd morseTrainer-dotnet

git clone git@github.com:alkisax/morseTrainer.git .
git checkout dotnet-signalr

dotnet --version

cd backend-dotnet
dotnet publish -c Release -o out

# test
cd out
dotnet backend-dotnet.dll
# είδα οτι δεν μου είχε κρατήσει το Port όπως το είχα βάλει στο \Properties\launchSettings.json. 
# πρόσθεσα   "Urls": "http://localhost:3016", στο backend-dotnet\appsettings.json
# και στο program cs
#var builder = WebApplication.CreateBuilder(args);
#// for port consistency on Hetzner
#builder.WebHost.UseUrls(
# builder.Configuration["Urls"]!
#);
# ανοίγω νεό τερμιναλ Hetzner
curl http://localhost:3016/api/ping

cd /var/www/morseTrainer-dotnet/backend-dotnet/out
pm2 start "dotnet backend-dotnet.dll" --name morse-dotnet

nano /etc/nginx/sites-available/morse-dotnet
cat /etc/nginx/sites-available/morse-dotnet

ln -s /etc/nginx/sites-available/morse-dotnet /etc/nginx/sites-enabled/

nginx -t
systemctl reload nginx
# browser test → http://morse-dotnet.portfolio-projects.space/health → ok

certbot --nginx -d morse-dotnet.portfolio-projects.space
```

## oneline deploy +
ssh root@49.12.76.128
```bash
cd /var/www/morseTrainer-dotnet && git pull origin dotnet-signalr && cd backend-dotnet && rm -rf out && dotnet publish -c Release -o out && pm2 restart morse-dotnet
```
pm2 logs morse-dotnet --lines 50
pm2 flush morse-dotnet