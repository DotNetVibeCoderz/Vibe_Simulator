# PhysicsNet ⚛️

**PhysicsNet** adalah 2D Physics Engine custom yang dibuat dengan **C#** dan **Avalonia UI**. Proyek ini mendemonstrasikan simulasi fisika rigid body dari awal tanpa menggunakan library fisika bawaan, berjalan lancar di Windows, Linux, dan macOS.

---

## 🇮🇩 Bahasa Indonesia

### Deskripsi
PhysicsNet mensimulasikan hukum fisika dasar seperti gravitasi, tumbukan, dan gaya pegas. Engine ini menggunakan *Impulse-based resolver* untuk menangani reaksi tabrakan dan *Separating Axis Theorem (SAT)* untuk deteksi tabrakan antar poligon (kotak).

### Fitur Utama
1.  **Dinamika Rigid Body**: Simulasi objek padat dengan massa, kecepatan, akselerasi, inersia, dan torsi.
2.  **Deteksi Tabrakan (Collision Detection)**:
    *   **Broad-phase**: Filter awal (belum diimplementasikan penuh, saat ini O(N^2)).
    *   **Narrow-phase**:
        *   Lingkaran vs Lingkaran.
        *   Lingkaran vs Kotak (Box).
        *   Kotak vs Kotak (Menggunakan SAT - Separating Axis Theorem).
3.  **Resolusi Tabrakan**: Menangani pantulan (*restitution*) dan gesekan (*friction*) agar objek memantul atau berhenti secara alami.
4.  **Joints & Constraints**:
    *   **Spring Joint**: Simulasi pegas/karet yang menghubungkan dua objek (lihat bola kuning di demo).
5.  **Multiplatform**: Menggunakan Avalonia UI untuk rendering, sehingga bisa jalan di OS manapun yang mendukung .NET.

### Persyaratan
*   .NET SDK (versi 8.0 atau lebih baru disarankan).

### Cara Menjalankan
1.  Buka terminal atau command prompt.
2.  Arahkan ke direktori project ini.
3.  Jalankan perintah berikut:
    ```bash
    dotnet run
    ```
4.  Nikmati simulasi fisika yang indah!

---

## 🇬🇧 English

### Description
PhysicsNet is a custom 2D Physics Engine built with **C#** and **Avalonia UI**. This project demonstrates rigid body physics simulation from scratch without using built-in physics libraries, running smoothly on Windows, Linux, and macOS.

### Key Features
1.  **Rigid Body Dynamics**: Simulates solid objects with mass, velocity, acceleration, inertia, and torque.
2.  **Collision Detection**:
    *   **Broad-phase**: Initial filtering (currently O(N^2)).
    *   **Narrow-phase**:
        *   Circle vs Circle.
        *   Circle vs Box.
        *   Box vs Box (Using SAT - Separating Axis Theorem).
3.  **Collision Resolution**: Handles restitution (bounciness) and friction logic so objects bounce or slide naturally.
4.  **Joints & Constraints**:
    *   **Spring Joint**: Simulates a spring connecting two bodies (see the yellow balls in the demo).
5.  **Multiplatform**: Uses Avalonia UI for rendering, ensuring compatibility with any OS supporting .NET.

### Requirements
*   .NET SDK (version 8.0 or newer recommended).

### How to Run
1.  Open your terminal or command prompt.
2.  Navigate to this project directory.
3.  Run the following command:
    ```bash
    dotnet run
    ```
4.  Enjoy the beautiful physics simulation!

---

### Credits
Created with ❤️ and ☕ by **Jacky the Code Bender**
*Gravicode Studios*

*Jangan lupa traktiran pulsanya ya bos! / Don't forget the treat!* 😉
