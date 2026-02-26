## Technical specification
### Classes & methods

#### `Contacts.Endpoints.ContactsEndpoints`
- `MapContactsEndpoints(IEndpointRouteBuilder app)` - registers `/api/contacts` endpoints (GET list, GET details, POST create, PATCH edit).
- `GetContacts(AppDbContext db)` - returns a contact list.
- `GetContactDetails(int id, AppDbContext db)` - returns contact details by (`id`).
- `CreateContact(CreateContactRequest req, AppDbContext db, PasswordHasher<Contact> hasher)` - validates input, hashes password, saves to DB.
- `EditContact(int id, UpdateContactRequest req, AppDbContext db, PasswordHasher<Contact> hasher)` - partial update (validates fields, updates category/subcategory/custom category), saves changes.

#### `Contacts.Endpoints.AuthEndpoints`
- `MapAuthEndpoints(IEndpointRouteBuilder app)` - registers authentication endpoints under `/api/auth`.
- `Register(RegisterRequest req, AppDbContext db, PasswordHasher<User> hasher)` - 
  validates email/password, checks email uniqueness, hashes the password, stores the user in the database.
- `Login(LoginRequest req, AppDbContext db, PasswordHasher<User> hasher, JwtTokenService tokens)` - validates input, generates a JWT access token using `JwtTokenService`, returns JWT `token`.

#### Database context `Contacts.Data.AppDbContext`
- `DbSet<Contact> Contacts`, `DbSet<Category> Categories`, `DbSet<Subcategory> Subcategories`, `DbSet<User> Users` - database tables mapping.
- `OnModelCreating(ModelBuilder modelBuilder)` - schema configuration and dictionary data seeding.

#### Entities (`Contacts.Data.Entities`)
- `User` - user entity.
- `Contact` - contact entity.
- `Category` - dictionary entity for categories.
- `Subcategory` - dictionary entity linked to `Category`.

#### DTOs (`Contacts.Dtos.*`)
- request/response models for endpoints.
- `JwtTokenService` - generates JWT access tokens for authenticated users `(used in Login)`.  

## Used libraries

### Backend (C# / .NET 8)
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT Bearer authentication
- **Microsoft.AspNetCore.OpenApi** - OpenAPI support for ASP.NET Core
- **Microsoft.EntityFrameworkCore** - ORM (Entity Framework Core)
- **Microsoft.EntityFrameworkCore.Design** - EF Core tooling (migrations)
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL provider for EF Core
- **Swashbuckle.AspNetCore** - Swagger UI

### Frontend (Vue SPA)
- **vue** - UI framework
- **vue-router** - client-side routing

## Architecture
- **Backend:** ASP.NET Core **REST API** implemented using **Minimal API**.
- **Frontend:** Single Page Application (SPA).
- **Database:** PostgreSQL (Docker), accessed via Entity Framework Core.

## Set up application

### Requirements
- .NET SDK 8.x
- Node.js LTS
- Docker

### Step by step (bash)
1. Clone into repository:
```bash 
git clone git@gitlab.com:lake-group/net-core.git 
```
2. Make sure to install dependencies  *If already installed continue from point 3* \
2.1 You can set up docker by following this quide: https://docs.docker.com/engine/install/ubuntu/#installation-methods \
2.2 .NET SDK and Node.js installation:
```bash
# .NET SDK
# at root directory
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Node.js
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.4/install.sh | bash
nvm install --lts
source ~/.bashrc # or restart bash terminal
```
3. Start up the docker container with database:
```bash
docker compose up -d
```
4. Setting up the backend:
```bash
dotnet restore
dotnet ef database update --project src/backend/Contacts/Contacts.csproj
dotnet run --project src/backend/Contacts/Contacts.csproj
```
5. Setting up the frontend:
```bash
cd src/frontend
npm install
npm run dev
```