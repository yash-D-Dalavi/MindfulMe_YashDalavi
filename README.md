# 🧠 MindfulMe - Mental Wellness Web Application

![Platform](https://img.shields.io/badge/Platform-Web-lightgrey)
![Framework](https://img.shields.io/badge/Framework-ASP.NET_MVC_5-blue)
![Language](https://img.shields.io/badge/Language-C%23-purple)
![Database](https://img.shields.io/badge/Database-SQL_Server-red)

MindfulMe is a comprehensive mental wellness web application designed specifically for college students. It provides a safe, interactive, and structured platform to track emotional health, connect with professional counselors, practice guided breathing exercises, and share feelings anonymously.

---

## ✨ Key Features

* **📊 Interactive Mood Tracker:** Log daily moods with emojis and notes. Visualizes a 30-day mood distribution using **Chart.js** and calculates consecutive login streaks using SQL CTEs.
* **🧑‍⚕️ Counselor Booking & Transactions:** Browse professional counselors, view their credentials, and book sessions seamlessly with simulated payment gateways (UPI, Card, Net Banking).
* **🌬️ Animated Breathing Exercises:** Interactive, CSS/JS-driven guided breathing exercises (e.g., Box Breathing, 4-7-8 method) with live countdown timers.
* **🎭 Anonymous Peer Support Wall:** A safe community forum where students can share feelings using randomly generated anonymous identities (e.g., "SilentOwl939") and mood tags.
* **🔐 Secure Authentication:** Built with ASP.NET Identity framework utilizing PBKDF2 password hashing and Anti-Forgery tokens (CSRF protection) for secure data entry.
* **⚙️ Comprehensive Admin Panel:** Full CRUD capabilities for managing counselors, user statistics, bookings, and moderating reported community posts.

---

## 🛠️ Technology Stack

| Component | Technology Used | Purpose |
| :--- | :--- | :--- |
| **Framework** | ASP.NET MVC 5 | Structural foundation and request routing |
| **Backend** | C# | Core business logic and controllers |
| **Database** | MS SQL Server | Relational data storage (8 connected tables) |
| **Data Access** | ADO.NET | Parameterized queries for secure DB connectivity |
| **Frontend UI** | HTML5, CSS3, Bootstrap | Responsive, gradient-themed user interface |
| **Interactivity** | JavaScript, Chart.js | Dynamic charts, animations, and form validations |

---

## 🏗️ Project Architecture

The application follows a strict **Layered MVC Architecture** to ensure clean separation of concerns:
1.  **Models:** Defines the data structure and business objects (e.g., `MoodEntry`, `Counselor`).
2.  **Views:** Razor pages (`.cshtml`) handling the presentation logic and UI.
3.  **Controllers:** Handles user requests, processes data, and returns the appropriate views.
4.  **Services Layer:** Encapsulates core business logic (e.g., calculating mood streaks, processing bookings).
5.  **DataAccess Layer:** Centralized `DBHelper.cs` utilizing `SqlParameter` to prevent SQL Injection attacks.

---

## 🗄️ Database Schema

The database (`MindfulMeDB`) consists of 8 interconnected tables maintaining full data integrity with Foreign Keys and CHECK constraints:
* `AspNetUsers` (Identity & Auth)
* `MoodEntries` (Daily tracking)
* `Counselors` & `CounselorBookings` (Transaction management)
* `Activities` & `ActivityCompletions` (Wellness tracking)
* `PeerPosts` & `PeerReplies` (Community forum)

---

## 🚀 Setup and Installation

Follow these steps to run the project locally:

1. **Clone the repository:**
   `git clone https://github.com/yash-D-Dalavi/MindfulMe_YashDalavi.git`
2. **Setup the Database:**
   * Open SQL Server Management Studio (SSMS).
   * Run the provided SQL script to create the `MindfulMeDB` database, tables, and insert sample data.
3. **Configure Connection String:**
   * Open the project in Visual Studio.
   * Go to `Web.config` and update the `ConnectionString` to match your local SQL Server instance.
4. **Run the Application:**
   * Press `F5` or click **Start** in Visual Studio.
   * Register a new user or use the default setup to explore the features!

---

## 👨‍💻 Developed By

**Yash Dalavi** *Computer Engineering Student* Passionate about building scalable, user-centric web applications and exploring modern backend architectures.
