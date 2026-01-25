# RagdollSim

**[Bahasa Indonesia]**

## Deskripsi
RagdollSim adalah aplikasi simulasi fisika sederhana yang dibuat menggunakan C# dan Windows Forms. Proyek ini mendemonstrasikan implementasi **Integrasi Verlet** untuk mensimulasikan gerakan fisik yang realistis pada model "boneka kain" (ragdoll).

Aplikasi ini dikembangkan oleh **Jacky The Code Bender** (AI dari Gravicode Studios).

## Fitur Utama
- **Simulasi Fisika Verlet:** Menggunakan metode integrasi Verlet untuk menghitung posisi dan kecepatan partikel tanpa menyimpan nilai kecepatan secara eksplisit, membuat simulasi lebih stabil.
- **Model Ragdoll:** Struktur tubuh manusia sederhana yang terdiri dari kepala, leher, badan, lengan, panggul, dan kaki yang saling terhubung dengan "sticks" (sambungan kaku).
- **Interaksi Pengguna:** Anda dapat menarik (drag) bagian tubuh ragdoll menggunakan mouse untuk melemparkan atau memposisikan boneka.
- **Gravitasi & Tabrakan:** Ragdoll merespons gravitasi dan memantul saat menabrak batas layar.
- **Kestabilan:** Menggunakan beberapa iterasi per frame untuk memastikan sambungan tetap kaku dan tidak melar.

## Persyaratan Sistem
- .NET 9.0 SDK atau lebih baru.
- Sistem Operasi Windows (karena menggunakan Windows Forms).

## Cara Menjalankan
1. Pastikan Anda memiliki .NET 9.0 SDK terinstal.
2. Buka folder proyek ini di terminal atau command prompt.
3. Jalankan perintah berikut untuk membangun dan menjalankan aplikasi:
   ```bash
   dotnet run
   ```
4. Atau, buka file `.csproj` menggunakan Visual Studio dan klik tombol **Start**.

## Cara Menggunakan
- **Klik Kiri dan Tahan** pada bagian tubuh (titik) mana pun dari ragdoll.
- **Gerakkan Mouse** untuk menarik ragdoll ke arah yang diinginkan.
- **Lepaskan Klik** untuk menjatuhkan ragdoll dan membiarkan fisika bekerja.

---

**[English]**

## Description
RagdollSim is a simple physics simulation application built using C# and Windows Forms. This project demonstrates the implementation of **Verlet Integration** to simulate realistic physical movements on a ragdoll model.

This application was developed by **Jacky The Code Bender** (AI from Gravicode Studios).

## Key Features
- **Verlet Physics Simulation:** Uses Verma integration methods to calculate particle positions and velocities explicitly without storing velocity values, making the simulation more stable.
- **Ragdoll Model:** A simple human body structure consisting of a head, neck, torso, arms, pelvis, and legs connected by rigid "sticks".
- **User Interaction:** You can drag ragdoll body parts using the mouse to fling or position the doll.
- **Gravity & Collision:** The ragdoll responds to gravity and bounces when hitting screen boundaries.
- **Stability:** Uses multiple iterations per frame to ensure connections remain rigid and do not stretch.

## System Requirements
- .NET 9.0 SDK or later.
- Windows Operating System (requires Windows Forms).

## How to Run
1. Ensure you have the .NET 9.0 SDK installed.
2. Open the project folder in a terminal or command prompt.
3. Run the following command to build and execute the application:
   ```bash
   dotnet run
   ```
4. Alternatively, open the `.csproj` file using Visual Studio and click the **Start** button.

## Controls
- **Left Click and Hold** on any body part (point) of the ragdoll.
- **Move Mouse** to drag the ragdoll around.
- **Release Click** to drop the ragdoll and let physics take over.
