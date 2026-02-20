## Set up/First time using application

### Requirements
- .NET SDK LTS
- Node.js LTS
- Docker

### Step by step
1. Clone into repository:
```bash 
git clone git@gitlab.com:lake-group/net-core.git 
```
2. Make sure to install dependecies \
2.1 You can set up docker by following this quide: https://docs.docker.com/engine/install/ubuntu/#installation-methods \
2.2 .NET SDK and Node.js installation:
```bash
# .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Node.js
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.4/install.sh | bash
nvm install --lts
source ~/.bashrc # or restart bash terminal
```
3. Get into src directory:
```bash
cd src
sudo docker compose up -d
sudo docker ps # list running containers
```
4. Get into backend directory:
```bash
dotnet restore
dotnet tool restore
dotnet ef database update
dotnet run
```
5. Get into frontend directory:
```bash
npm install
npm run dev
```