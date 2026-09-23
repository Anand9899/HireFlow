# HireFlow 🚀

> **Next-Generation Career & Recruitment Platform**

HireFlow is a modern, full-stack recruitment and job discovery web application built with **ASP.NET Core Web API**, **Entity Framework Core**, **SQLite**, and modern frontend technologies (HTML5, Vanilla CSS3 with glassmorphism, responsive design, and JavaScript).

---

## ✨ Features

- **Candidate Discovery & Job Explorer:** Real-time search, category filtering (Full Stack, Frontend, Backend, AI/ML, DevOps, Cloud, Cybersecurity, etc.), and location/salary badges.
- **Interactive Candidate Application System:**
  - Multi-section application form with Personal, Professional, and Academic records.
  - Strict PDF & DOCX validation for Resume uploads.
  - Academic records tracking (10th, 12th, UG, PG percentage marks).
  - Current location and job profile mapping.
- **Recruiter Command Center & Dashboard:**
  - Post new job vacancies with custom job types, salaries, and descriptions.
  - Review applicant submissions in real time with structured candidate profiles, academic records, and resume links.
  - Manage candidate statuses (`Pending`, `Accepted / Shortlisted`, `Rejected / Declined`).
- **Authentication & Security:**
  - JWT (JSON Web Token) bearer authentication with role-based authorization (`Candidate` vs `Recruiter`).
  - Secure password hashing and credential verification.
- **Modern Responsive UI:**
  - Dark / Light mode toggle.
  - Glassmorphic navigation, sticky navbar, and animated micro-interactions.
  - Fast single-page architecture with live filtering.

---

## 🛠️ Tech Stack

- **Backend:** C# / ASP.NET Core 10 Web API
- **ORM & Database:** Entity Framework Core, SQLite (`HireFlow.db`)
- **Authentication:** JWT Bearer Authentication
- **Frontend:** Vanilla HTML5, CSS3, Modern ES6+ JavaScript
- **API Architecture:** RESTful endpoints for Auth, Jobs, and Job Applications

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/YOUR_USERNAME/HireFlow.git
   cd HireFlow/HireFlow
   ```

2. Restore dependencies and run:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to:
   ```
   http://localhost:5036
   ```

---

## 📄 License
This project is open source and available under the [MIT License](LICENSE).
