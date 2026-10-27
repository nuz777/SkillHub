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
| **Employee (Instructor)** | Carlos Mendoza | `carlos.mendoza@skillhub.com` | `Instructor123!` | Fotografía |
| **Employee (Instructor)** | Laura Gómez | `laura.gomez@skillhub.com` | `Instructor123!` | Cocina |
| **Employee (Instructor)** | Andrés Torres | `andres.torres@skillhub.com` | `Instructor123!` | Oratoria |
| **Employee (Instructor)** | Sofía Morales | `sofia.morales@skillhub.com` | `Instructor123!` | Diseño Gráfico |

### Participant Accounts (Access to Participant Web Portal & Public Endpoints)
| Role | Full Name | Email | Password |
|---|---|---|---|
| **Participant** | Juan Pérez | `juan.perez@example.com` | `Participant123!` |
| **Participant** | María Rodríguez | `maria.rodriguez@example.com` | `Participant123!` |
| **Participant** | Carlos López | `carlos.lopez@example.com` | `Participant123!` |
| **Participant** | Ana Martínez | `ana.martinez@example.com` | `Participant123!` |

---

## ⚙️ Setup & Execution Instructions

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [JetBrains Rider](https://www.jetbrains.com/rider/) (or Visual Studio / VS Code)
- [PostgreSQL](https://www.postgresql.org/) running locally or in Docker.

### 2. Database Setup
Create a PostgreSQL database named `skillhub_db`:
```bash
createdb -U postgres -h localhost skillhub_db
```
*(Or execute the provided [skillhub_database.sql](skillhub_database.sql) script directly in PostgreSQL/pgAdmin/DBeaver).*

Verify the connection string in `SkillHub.Api/appsettings.json` and `SkillHub.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=skillhub_db;Username=postgres;Password=postgres"
}
```

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
| `POST` | `/api/v1/workshops` | Employee | Publishes a new workshop |
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
