# Golden Bridge Traffic Simulator 🌉🚗

![Status](https://img.shields.io/badge/Status-Prototype-green) ![Blazor](https://img.shields.io/badge/Framework-Blazor_WASM-purple) ![ThreeJS](https://img.shields.io/badge/Graphics-Three.js-black)

## 🇬🇧 English

### Description
Golden Bridge Traffic Simulator is a web-based 3D simulation application built with **Blazor WebAssembly** and **Three.js**. It simulates traffic flow on a large suspension bridge with realistic features such as day/night cycles, weather effects (rain), and basic vehicle AI.

### Key Features
*   **Immersive 3D Graphics**: Rendered using Three.js with shadows, lighting, and geometric modeling.
*   **Dynamic Day/Night Cycle**: Control the time of day and watch the lighting change.
*   **Weather Systems**: Toggle between clear skies and rain effects.
*   **Interactive Controls**: Adjust simulation speed and spawn vehicles manually via the C# Blazor interface.
*   **Cross-Platform**: Runs in any modern web browser.

### How to Run
1.  Ensure you have **.NET 6.0 SDK** or later installed.
2.  Open the project folder in your terminal.
3.  Run the command:
    ```bash
    dotnet watch
    ```
4.  Open your browser at `https://localhost:5001` (or the port shown in terminal).

### Architecture
*   **Frontend Logic (C#)**: Handles UI state, user input, and simulation parameters.
*   **Graphics Engine (JavaScript)**: Handles the high-performance render loop, Three.js scene graph, and particle systems for weather.
*   **Interop**: Blazor communicates with the JS engine via `IJSRuntime`.

---

## 🇮🇩 Bahasa Indonesia

### Deskripsi
Golden Bridge Traffic Simulator adalah aplikasi simulasi 3D berbasis web yang dibuat dengan **Blazor WebAssembly** dan **Three.js**. Aplikasi ini mensimulasikan arus lalu lintas di jembatan gantung besar dengan fitur realistis seperti siklus siang/malam, efek cuaca (hujan), dan AI kendaraan dasar.

### Fitur Utama
*   **Grafis 3D Imersif**: Dirender menggunakan Three.js dengan bayangan, pencahayaan, dan pemodelan geometris.
*   **Siklus Siang/Malam Dinamis**: Atur waktu dan lihat perubahan pencahayaannya.
*   **Sistem Cuaca**: Ubah kondisi antara cerah dan hujan.
*   **Kontrol Interaktif**: Sesuaikan kecepatan simulasi dan munculkan kendaraan secara manual melalui antarmuka C# Blazor.
*   **Lintas Platform**: Berjalan di browser web modern apa pun.

### Cara Menjalankan
1.  Pastikan Anda telah menginstal **.NET 6.0 SDK** atau yang lebih baru.
2.  Buka folder proyek di terminal Anda.
3.  Jalankan perintah:
    ```bash
    dotnet watch
    ```
4.  Buka browser Anda di `https://localhost:5001` (atau port yang ditampilkan di terminal).

### Arsitektur
*   **Logika Frontend (C#)**: Menangani status UI, input pengguna, dan parameter simulasi.
*   **Mesin Grafis (JavaScript)**: Menangani loop render performa tinggi, scene graph Three.js, dan sistem partikel untuk cuaca.
*   **Interop**: Blazor berkomunikasi dengan mesin JS melalui `IJSRuntime`.

---
*Created by Jacky the Code Bender - Gravicode Studios*
*Jangan lupa traktir pulsanya ya! 😉*
