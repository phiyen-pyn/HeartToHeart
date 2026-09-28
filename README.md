# 💜 HeartToHeart — Mental Health Care System

> **SU25 EXE201 | Summer 2025 Capstone Project**

HeartToHeart is a comprehensive **mental health care platform** built as a RESTful Web API backend using **ASP.NET Core 8.0**. It connects users with professional counselors, provides AI-powered mental health support, and offers a suite of wellness tools to help individuals manage their emotional wellbeing.

---

## 🌟 Key Features

### 👤 User & Authentication
- **JWT-based Authentication** with refresh token support
- **Google OAuth 2.0** login integration
- Role-based authorization (User, Counselor, Admin)
- Email verification and password management

### 🧠 AI Mental Health Support
- **AI Conversation** powered by OpenRouter API — users can chat with an AI assistant for mental health guidance
- Contextual, empathetic responses tailored to mental wellness

### 📅 Counselor Appointment System
- Browse and book sessions with licensed counselors
- **Counselor availability management** with schedule templates
- **Google Calendar integration** for counselors to sync appointments
- Holiday-aware scheduling with automated holiday seeding
- Appointment history tracking

### 📊 Emotion & Wellness Tracking
- **Emotion tracking** — log and monitor daily emotional states
- **Personal diary entries** for private journaling
- Visual insights into emotional patterns over time

### 📝 Community & Content
- **Anonymous posts** — share thoughts without revealing identity
- **Blog system** with tags, likes, and comments
- Community support through shared experiences

### 💳 Subscription & Payments
- **Subscription plans** for premium features
- Integrated **VNPay payment gateway** for secure transactions
- Automated subscription expiry via scheduled jobs (Quartz.NET)

### 🖼️ Media & Storage
- **Firebase Storage** integration for image uploads
- Support for JPEG, PNG, GIF, WebP formats (up to 10MB)

### 📧 Email Notifications
- SMTP-based email service (Gmail) for notifications, OTP, and confirmations

### 📈 Dashboard & Reporting
- Admin dashboard with platform analytics
- User and counselor activity reports

---

## 🏗️ Architecture

The project follows a clean **N-Tier / Layered Architecture**:

```
HeartToHeart_MentalHealthCareSystem/
├── EXE201.HeartToHeart.WebApi      # Presentation Layer — Controllers, Jobs, Middleware
├── EXE201.HeartToHeart.BLL         # Business Logic Layer — Services, Interfaces
├── EXE201.HeartToHeart.DAL         # Data Access Layer — Entities, Repositories, EF Core Migrations
└── EXE201.HeartToHeart.Common      # Shared Layer — Constants, Utilities
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core 8.0 |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | Microsoft SQL Server (Azure SQL) |
| **Authentication** | ASP.NET Core Identity + JWT Bearer + Google OAuth |
| **Payment** | VNPay (.NET SDK) |
| **Storage** | Firebase Storage |
| **AI Integration** | OpenRouter API |
| **Calendar** | Google Calendar API v3 |
| **Scheduling** | Quartz.NET 3.14 |
| **Documentation** | Swagger / OpenAPI (Swashbuckle) |
| **Email** | SMTP (Gmail) |
| **Hosting** | Azure App Service + Azure SQL Database |

---

## 📦 Core Entities

| Entity | Description |
|---|---|
| `ApplicationUser` | Platform users with identity |
| `Counselor` | Registered mental health counselors |
| `Appointment` | Booked counseling sessions |
| `CounselorAvailability` | Counselor time slots |
| `AIConversation` / `ChatMessage` | AI chat history |
| `EmotionTrack` | Daily emotion logs |
| `DiaryEntry` | Private journal entries |
| `AnonymousPost` | Community anonymous sharing |
| `Blog` / `BlogComment` / `BlogLike` | Blog content system |
| `Subscription` | User subscription plans |
| `PaymentTransaction` | VNPay payment records |
| `Notification` | User notification system |
| `Report` | Content reporting |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or Azure)
- Firebase project (for image uploads)
- Google Cloud project (for OAuth & Calendar API)

### Configuration

Update `appsettings.json` with your credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Initial Catalog=HeartToHeartDB;..."
  },
  "Jwt": {
    "Key": "YOUR_JWT_SECRET_KEY",
    "Issuer": "https://your-api-url.com",
    "Audience": "https://your-api-url.com",
    "ExpiryInHours": 24
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  },
  "FirebaseConfig": {
    "ProjectId": "your-firebase-project",
    "StorageBucket": "your-firebase-project.appspot.com",
    "ServiceAccountKeyPath": "./serviceAccountKey.json"
  },
  "OpenRouter": {
    "ApiKey": "YOUR_OPENROUTER_API_KEY"
  },
  "Vnpay": {
    "TmnCode": "YOUR_VNPAY_TMN_CODE",
    "HashSecret": "YOUR_VNPAY_HASH_SECRET"
  }
}
