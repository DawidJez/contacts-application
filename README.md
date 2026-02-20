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
```
3. Get into src directory:
```bash
docker compose up -d
docker ps
```
4. Get into backend directory:
```bash
dotnet restore
dotnet ef database update
dotnet run
```
5. Get into fronted directory:
```bash
npm install
npm run dev
```