# SkillHub - In-Person Workshop & Enrollment Management System

SkillHub is a robust training center platform designed to manage short, in-person workshops across various disciplines (such as photography, cooking, public speaking, and design) and streamline participant registrations. It eliminates overbooking, duplicate enrollments, and scheduling conflicts through strong business rules modeled within a Clean Architecture solution.

---

## 🚀 Architecture Overview

The solution strictly adheres to **Clean Architecture** and SOLID design principles, dividing responsibilities into distinct layers where dependencies point inwards:

- **`SkillHub.Domain`**: Core domain entities (`Workshop`, `Enrollment`, `Instructor`, `BaseEntity`), enums (`EnrollmentStatus`, `UserRoles`), repository contracts, and domain business rules and validation logic.
- **`SkillHub.Application`**: Use cases, business logic orchestrations, DTOs, interface abstractions (`IAuthService`, `IWorkshopService`, `IEnrollmentService`, `ICertificatePdfGenerator`, `IExcelReader`), and data mapping.
- **`SkillHub.Infrastructure`**: Implementation of external concerns, EF Core `ApplicationDbContext` with PostgreSQL (`Npgsql`), ASP.NET Core Identity integration, JWT token generator, EPPlus for Excel mass import, and QuestPDF for official enrollment certificate generation.
- **`SkillHub.Api`**: RESTful Web API exposing versioned endpoints (`/api/v1/*`), protected by JWT Bearer tokens and role-based authorization (`Employee`, `Participant`), complete with interactive **Scalar** (`/scalar`) and **Swagger UI** (`/swagger`) documentation.
- **`SkillHub.Web`**: Responsive participant presentation layer built with ASP.NET Core MVC (Option A). Directly consumes Application layer abstractions, featuring registration, login, workshop catalog with live spot indicators, enrollment management, cancellation, and PDF certificate downloads.

---

## 🛠️ Technologies & Libraries

- **Framework**: .NET 10 (C# 13)
- **Data Persistence**: Entity Framework Core 10.0 + PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Authentication & Security**: ASP.NET Core Identity + JWT Bearer Tokens (`System.IdentityModel.Tokens.Jwt`)
- **API Documentation**: OpenAPI / Swagger (`Swashbuckle.AspNetCore`) and **Scalar API Reference** (`Scalar.AspNetCore` / CDN)
- **Document Generation**: **QuestPDF** (High-performance C# PDF generation for enrollment certificates)
- **Data Import**: **EPPlus** (OpenXML Excel processing for mass enrollments)
- **Frontend (Web)**: ASP.NET Core MVC, Bootstrap 5, Bootstrap Icons

---

## 📋 Business Rules Implemented

1. **Max Capacity**: Active enrollments cannot exceed the maximum capacity configured for a workshop. Available spots are calculated in real time (`AvailableSpots = MaxCapacity - ActiveEnrollmentsCount`).
2. **No Duplicates**: A participant cannot hold two active enrollments in the same workshop.
3. **No Schedule Overlaps**: A participant cannot enroll in a workshop that overlaps in time with another workshop in which they are already actively enrolled (`StartA < EndB && StartB < EndA`).
4. **Future Workshops Only**: Participants can only enroll in future workshops (`StartDate > UtcNow`).
5. **Cancellation Policies**:
   - **Participant**: Can cancel their enrollment up to 24 hours prior to the workshop start date/time. Cancellation releases the reserved spot.
   - **Employee**: Can cancel any participant's enrollment without the 24-hour restriction, provided the workshop has not already started.
   - Every cancellation logs who performed it (`CancelledBy`) and when (`CancelledAt`).
6. **Re-enrollment**: A participant who previously cancelled can re-enroll in the same workshop if capacity is available and no other active conflict exists.

---

## 🔐 Credentials & Default Accounts

The database seeds initial employee (instructor) and participant accounts automatically upon startup:

### Employee / Instructor Accounts (Access to API, Scalar, and Management Endpoints)
| Role | Full Name | Email | Password | Specialty |
|---|---|---|---|---|
| **Employee (Admin)** | Admin SkillHub | `admin@skillhub.com` | `Admin123!` | Coordinación |
| **Employee (Instructor)** | Andrés Felipe Mejía Arango | `andres.mejia@correo.com` | `Instructor2026*` | Fotografía |
| **Employee (Instructor)** | Rosa Elena Barrios Polo | `rosa.barrios@correo.com` | `Instructor2026*` | Cocina |
| **Employee (Instructor)** | Mauricio Jiménez Cárdenas | `mauricio.jimenez@correo.com` | `Instructor2026*` | Oratoria |
| **Employee (Instructor)** | Natalia Herrera Castaño | `natalia.herrera@correo.com` | `Instructor2026*` | Diseño gráfico |
| **Employee (Instructor)** | Hernán Darío Quintero Gil | `hernan.quintero@correo.com` | `Instructor2026*` | Música |
| **Employee (Instructor)** | Paola Andrea Vargas Mora | `paola.vargas@correo.com` | `Instructor2026*` | Marketing digital |

### Participant Accounts (Access to Participant Web Portal & Public Endpoints)
| Role | Full Name | Email | Password |
|---|---|---|---|
| **Participant** | Juan Pérez | `juan.perez@example.com` | `Participant123!` |
| **Participant** | María Rodríguez | `maria.rodriguez@example.com` | `Participant123!` |
| **Participant** | Carlos López | `carlos.lopez@example.com` | `Participant123!` |
| **Participant** | Ana Martínez | `ana.martinez@example.com` | `Participant123!` |

### 🔑 Ready-to-Use JWT Bearer Token (Employee Admin)
For convenience during evaluation or testing directly in **Scalar** (`/scalar`) or **Swagger UI** (`/swagger`):
```text
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIzNDAxZTY5OS0xYTA3LTQyZjgtODQwMC01NDkzOWY0NjQxNjciLCJlbWFpbCI6ImFkbWluQHNraWxsaHViLmNvbSIsInVuaXF1ZV9uYW1lIjoiQWRtaW4gU2tpbGxIdWIiLCJyb2xlIjoiRW1wbG95ZWUiLCJqdGkiOiI2ZjM0NTkyNy1lYTM4LTQzNGMtOTBhNS1mMmJhODFjMjVjYTgiLCJuYmYiOjE3OTMxMzA5MDAsImV4cCI6MTc5MzIxNzMwMCwiaWF0IjoxNzkzMTMwOTAwLCJpc3MiOiJTa2lsbEh1YiIsImF1ZCI6IlNraWxsSHViLlVzZXJzIn0.ZVFSM9yiIpax6lrUSj1sdBPMyo3BoQkC2OKSDyOnqnI
```

---

## ⚙️ Setup & Execution Instructions

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [JetBrains Rider](https://www.jetbrains.com/rider/) (or Visual Studio / VS Code)
- [PostgreSQL](https://www.postgresql.org/) running locally or in Docker.

### 2. Database Setup (Dockerized PostgreSQL - `SkillHubDb`)
Start the PostgreSQL container named **`SkillHubDb`** with automatic schema and seed data initialization:

```bash
# Option A: Using Docker Compose (Recommended)
docker compose up -d

# Option B: Direct Docker CLI command
docker run -d \
  --name SkillHubDb \
  -p 5432:5432 \
  -e POSTGRES_DB=SkillHubDb \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -v skillhub_postgres_data:/var/lib/postgresql/data \
  -v $(pwd)/skillhub_database.sql:/docker-entrypoint-initdb.d/01-init.sql \
  postgres:16-alpine
```

Verify the connection string in `SkillHub.Api/appsettings.json` and `SkillHub.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=SkillHubDb;Username=postgres;Password=postgres"
}
```
*(Automatic Resiliency: If PostgreSQL on port 5432 is not running, EF Core automatically falls back to SQLite `skillhub.db` in the solution root without crashing).*

### 3. Running with JetBrains Rider
1. Open the solution file: **`SkillHub.sln`**.
2. To run the REST API:
   - Select the run configuration **`SkillHub.Api: http`**.
   - Click **Run** or **Debug** (`Shift + F10`).
   - Open your browser at **`http://localhost:5180/scalar`** to interact with the API via the Scalar dashboard, or **`http://localhost:5180/swagger`** for Swagger UI.
3. To run the Participant Web portal:
   - Select the run configuration **`SkillHub.Web: http`**.
   - Click **Run** (`Shift + F10`).
   - Open your browser at **`http://localhost:5181`** to explore workshops, sign in as a participant, enroll, and download certificates.

### 4. Running via Terminal (CLI)
```bash
# Run the API
dotnet run --project SkillHub.Api/SkillHub.Api.csproj

# In another terminal, run the Web application
dotnet run --project SkillHub.Web/SkillHub.Web.csproj
```

---

## 📡 REST API Endpoints Specification

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/v1/auth/register` | Public | Registers a new participant account |
| `POST` | `/api/v1/auth/login` | Public | Authenticates and returns a JWT token |
| `GET` | `/api/v1/workshops` | Public | Lists all future workshops with calculated available spots |
| `GET` | `/api/v1/workshops/{id}` | Public | Retrieves details of a specific workshop |
| `POST` | `/api/v1/workshops` | Employee | Publishes a new workshop (instructor email auto-assigned if omitted) |
| `PUT`  | `/api/v1/workshops/{id}` | Instructor / Admin | Updates a workshop (validating ownership, future dates, and minimum capacity) |
| `DELETE`| `/api/v1/workshops/{id}`| Instructor / Admin | Deletes an unstarted workshop without active enrollments |
| `GET` | `/api/v1/workshops/{id}/enrollments` | Employee | Lists enrolled participants for a given workshop |
| `POST` | `/api/v1/enrollments` | Participant | Enrolls the authenticated participant in a workshop |
| `GET` | `/api/v1/enrollments/mine` | Participant | Lists enrollments of the authenticated participant |
| `PATCH`| `/api/v1/enrollments/{id}/cancel` | Owner or Employee | Cancels an enrollment (validating 24h rule) |
| `POST` | `/api/v1/enrollments/import` | Employee | Imports enrollments in bulk from an Excel file (`.xlsx`) |
| `GET` | `/api/v1/enrollments/{id}/certificate` | Owner or Employee | Generates and downloads the official PDF certificate |
| `GET` | `/api/v1/reports/top-workshops` | Employee | Reports workshops ordered by active enrollments descending |

---

## 👤 Author Information

- **Full Name**: [Tu Nombre Completo]
- **Clan**: [Tu Clan / Cohorte]
- **Email**: [Tu Correo Electrónico]
- **Identification Document**: [Tu Documento de Identidad]
