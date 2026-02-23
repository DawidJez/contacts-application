## Set up/First time using application

### Requirements
- .NET SDK 8.x
- Node.js LTS
- Docker

### Step by step
1. Clone into repository:
```bash 
git clone git@gitlab.com:lake-group/net-core.git 
```
2. Make sure to install dependencies  *If already installed continue from point 3* \
2.1 You can set up docker by following this quide: https://docs.docker.com/engine/install/ubuntu/#installation-methods \
2.2 .NET SDK and Node.js installation:
```bash
# .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Node.js
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.4/install.sh | bash
nvm install --lts
source ~/.bashrc # or restart bash terminal
```
3. Start up the docker container:
```bash
sudo docker compose up -d
```
4. Setting up the backend:
```bash
dotnet restore
dotnet ef database update
cd src/backend/Contacts/ && dotnet run
```
5. Get into frontend directory:
```bash
npm install
npm run dev
```