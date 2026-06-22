# TabibLens

An AI-powered **.NET 8 Web API** for scanning handwritten prescriptions via OCR, extracting medications, and providing an AI chat assistant for pharmaceutical Q&A.

## Architecture

Clean Architecture with 4 layers:

```
┌─────────────────────────────────────────────────────────┐
│  API Layer (Controllers, Program.cs, Config)            │
├─────────────────────────────────────────────────────────┤
│  Application Layer (Services, DTOs)                     │
├─────────────────────────────────────────────────────────┤
│  Infrastructure Layer (Repositories, DbContext, APIs)   │
├─────────────────────────────────────────────────────────┤
│  Domain Layer (Entities, Enums, Interfaces)             │
└─────────────────────────────────────────────────────────┘
```

### Domain Layer (`Domain/`)

Core business entities and interfaces — **zero dependencies**.

| Entities | Enums | Interfaces |
|---|---|---|
| `User` | `PrescriptionStatus` | `IRepository<T>` (base CRUD) |
| `RefreshToken` | `DosageForm` | `IUserRepository` |
| `Prescription` | `MessageRole` | `IPrescriptionRepository` |
| `Medication` | | `IRefreshTokenRepository` |
| `ChatSession` | | `IChatSessionRepository` |
| `ChatMessage` | | `IChatMessageRepository` |
| `BaseEntity` (abstract) | | `IMedicationRepository` |
| | | `IUnitOfWork` |
| | | `IOcrService` |
| | | `IChatAiService` |

All entities inherit from `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt`).

### Infrastructure Layer (`Infra/`)

| Component | Description |
|---|---|
| `AppDbContext` | EF Core context (PostgreSQL), implements `IUnitOfWork`, auto-sets `CreatedAt`/`UpdatedAt` |
| `RepositoryBase<T>` | Generic CRUD with soft-delete support |
| 6 Repositories | `UserRepository`, `PrescriptionRepository`, `RefreshTokenRepository`, `ChatSessionRepository`, `ChatMessageRepository`, `MedicationRepository` |
| `QwenOcrService` | HuggingFace Qwen2.5-VL-7B for prescription image OCR |
| `GroqChatService` | Groq LLaMA 3.3 70B for pharmaceutical AI chat |

### Application Layer (`Application/`)

| Services | Description |
|---|---|
| `AuthService` | JWT login/register, BCrypt password hashing, refresh token rotation |
| `ChatService` | AI chat sessions with prescription-aware context |
| `PrescriptionService` | OCR scanning, medication parsing, status management |

**13 DTOs**: `LoginDto`, `RegisterDto`, `UserDto`, `JwtSettings`, `ChatRequestDto`, `ChatResponseDto`, `ChatSessionDto`, `ChatMessageDto`, `OcrRequestDto`, `OcrResultDto`, `PrescriptionDto`, `PrescriptionWithMedicationsDto`, `MedicationDto`

### API Layer (`TabibLens.Api/`)

| Controller | Endpoints | Auth |
|---|---|---|
| `AuthController` | `POST /api/auth/login`, `POST /api/auth/register`, `POST /api/auth/logout` | Login/Register public; Logout requires JWT |
| `ChatController` | `POST /api/chat/sessions`, `GET /api/chat/sessions`, `GET /api/chat/sessions/{id}/messages`, `POST /api/chat/sessions/{id}/messages`, `DELETE /api/chat/sessions/{id}` | All require JWT |
| `PrescriptionController` | `POST /api/prescription/scan`, `GET /api/prescription`, `GET /api/prescription/{id}`, `GET /api/prescription/{id}/medications`, `GET /api/prescription/status/{status}`, `GET /api/prescription/{id}/result`, `POST /api/prescription/{id}/parse`, `PATCH /api/prescription/{id}/status`, `DELETE /api/prescription/{id}` | All require JWT |

---

## Tech Stack

| Technology | Purpose |
|---|---|
| .NET 8 | Web API framework |
| PostgreSQL | Database |
| Entity Framework Core | ORM |
| BCrypt.Net | Password hashing |
| JWT Bearer | Authentication |
| HuggingFace API | Prescription OCR (Qwen2.5-VL-7B) |
| Groq API | Pharmaceutical AI Chat (LLaMA 3.3 70B) |
| Swagger | API documentation |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)

### Configuration

Edit `TabibLens.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TabibLens;Username=postgres;Password=YOUR_PASSWORD"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_AT_LEAST_32_CHARS",
    "Issuer": "TabibLens",
    "Audience": "TabibLens-Client",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "HuggingFace": {
    "ApiKey": "YOUR_HUGGINGFACE_API_KEY",
    "ModelId": "Qwen/Qwen2.5-VL-7B-Instruct"
  },
  "Groq": {
    "ApiKey": "YOUR_GROQ_API_KEY",
    "Model": "llama-3.3-70b-versatile",
    "Temperature": 0.7,
    "MaxTokens": 1024
  }
}
```

### Run

```bash
# Restore packages
dotnet restore

# Apply migrations (create database)
dotnet ef database update --project Infra --startup-project TabibLens.Api

# Run the API
dotnet run --project TabibLens.Api
```

The API will be available at `https://localhost:7xxx` with Swagger UI at `/swagger`.

---

## Authentication Flow

```
┌──────────┐     POST /api/auth/register      ┌──────────┐
│  Client  │ ──────── or ────────────────────► │   API    │
│          │     POST /api/auth/login          │          │
│          │ ◄──────────────────────────────── │          │
│          │   Body: { UserDto + AccessToken } │          │
│          │   Cookie: refreshToken (HttpOnly) │          │
│          │                                   │          │
│          │     Authorization: Bearer <JWT>    │          │
│          │ ─────────────────────────────────► │          │
│          │   (All protected endpoints)       │          │
└──────────┘                                   └──────────┘
```

- **Access Token**: Short-lived JWT (15 min) sent in response body
- **Refresh Token**: Long-lived (7 days), SHA256-hashed in DB, sent as HTTP-only secure cookie
- **Passwords**: BCrypt-hashed with work factor 12

---

## Prescription Processing Pipeline

```
┌───────────┐    ┌─────────────┐    ┌──────────────┐    ┌──────────────┐
│  Upload   │ ─► │  OCR Scan   │ ─► │  Parse JSON  │ ─► │  Store Meds  │
│  Image    │    │ (HuggingFace)│    │  Medications │    │  in DB       │
└───────────┘    └─────────────┘    └──────────────┘    └──────────────┘
     │                                                         │
     └── Status: Uploaded ──► OcrProcessing ──► Parsed/Failed ─┘
```

1. User uploads a prescription image via `POST /api/prescription/scan`
2. Image is sent to HuggingFace Qwen2.5-VL for OCR
3. OCR returns a JSON array of extracted medications
4. Medications are parsed and stored in the database
5. Prescription status transitions: `Uploaded → OcrProcessing → Parsed/PartiallyParsed/Failed`

---

## AI Chat

Chat sessions can optionally be linked to a prescription. When linked, the AI assistant receives medication context to provide prescription-aware answers about drug interactions, side effects, and usage instructions.

The pharmaceutical assistant:
- Answers in the language of the user's message (Arabic/English)
- Provides evidence-based medication information
- Always advises consulting a healthcare professional

---

## Project Structure

```
AI-Agent-for-Prescription-Fulfillment/
├── Domain/
│   ├── Entities/              # User, Prescription, Medication, ChatSession, etc.
│   ├── Enums/                 # PrescriptionStatus, DosageForm, MessageRole
│   ├── Interfaces/            # IOcrService, IChatAiService
│   └── Repository Interfaces/ # IRepository<T>, IUserRepository, etc.
│
├── Application/
│   ├── DTOs/                  # Request/response models
│   └── Services/
│       ├── Abstraction/       # IAuthService, IChatService, IPrescriptionService
│       └── Implementation/    # AuthService, ChatService, PrescriptionService
│
├── Infra/
│   ├── Data/                  # AppDbContext + EF Configurations
│   ├── Repositories/          # Repository implementations
│   └── ExternalApis/          # HuggingFace (OCR) + Groq (Chat AI)
│
└── TabibLens.Api/           # API entry point
    ├── Controllers/           # Auth, Chat, Prescription
    ├── Program.cs             # DI, middleware, JWT config
    └── appsettings.json       # Configuration
```

---

## Known Issues & TODOs

> [!IMPORTANT]
> The following items should be addressed before production deployment.

| Issue | Severity | Location | Description |
|---|---|---|---|
| Missing `SaveChangesAsync` in AuthService | 🔴 Critical | `AuthService.cs` | `LoginAsync` and `RegisterAsync` call `AddAsync` / `Update` but never call `_unitOfWork.SaveChangesAsync()` — changes are **not persisted** to the database |
| No `IUnitOfWork` injected in AuthService | 🔴 Critical | `AuthService.cs` | The service has no `IUnitOfWork` dependency, so it cannot commit transactions |
| No global exception handling | 🟡 Medium | API layer | Controllers use try/catch per action; a global exception middleware would be cleaner and prevent unhandled 500s from leaking stack traces |
| `RefreshTokenAsync` commented out | 🟡 Medium | `IAuthService.cs` | The interface still has the method commented out — should be removed or implemented |
| No ownership checks in PrescriptionController | 🟡 Medium | `PrescriptionController.cs` | `GetById`, `Delete`, `UpdateStatus` don't verify the prescription belongs to the requesting user |
| API keys in `appsettings.json` | 🟡 Medium | `appsettings.json` | HuggingFace and Groq API keys are committed in plaintext — should use User Secrets or environment variables |
| No EF migrations present | 🟡 Medium | `Infra/` | No `Migrations/` folder — database needs initial migration created |
| `CreateSessionRequest` DTO in controller | 🔵 Low | `ChatController.cs` | Should be moved to `Application/DTOs/` for consistency |
