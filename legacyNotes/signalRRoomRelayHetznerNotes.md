Στο Namecheap :

Type: A
Host: signalr-room-relay
Value: 49.12.76.128
TTL: Automatic

ssh root@49.12.76.128

cd /var/www
mkdir signalr-room-relay
cd signalr-room-relay
git clone git@github.com:alkisax/signalr-room-relay.git .
cd backend-dotnet

-- test
cd /var/www/signalr-room-relay/backend-dotnet
dotnet publish -c Release -o out

cd /var/www/signalr-room-relay/backend-dotnet/out
dotnet backend-dotnet.dll