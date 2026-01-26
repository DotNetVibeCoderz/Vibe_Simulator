# RainSim 🌧️

**(English Version below)**

---

## 🇮🇩 Bahasa Indonesia

**RainSim** adalah aplikasi simulasi hujan interaktif yang dibuat menggunakan C# dan Windows Forms (.NET). Aplikasi ini mendemonstrasikan animasi partikel sederhana dengan performa yang mulus (~60 FPS) dan efek paralaks untuk memberikan kesan kedalaman 3D.

Dibuat dengan ❤️ oleh **Jacky The Code Bender**.

### Fitur Utama
*   **Efek Paralaks**: Tetesan hujan memiliki "kedalaman" (Z-depth) yang berbeda. Tetesan yang "jauh" terlihat lebih kecil, lebih transparan, dan jatuh lebih lambat dibandingkan tetesan yang "dekat".
*   **Kontrol Angin**: Anda dapat mengatur arah dan kekuatan angin secara real-time. Geser ke kiri untuk angin dari kanan, dan sebaliknya.
*   **Kontrol Gravitasi**: Mengatur seberapa cepat hujan jatuh ke tanah.
*   **Intensitas Hujan**: Mengatur jumlah partikel hujan, dari gerimis ringan (100 partikel) hingga badai lebat (3000 partikel).
*   **Responif**: Tampilan menyesuaikan ukuran jendela aplikasi.

### Cara Menjalankan
1.  Pastikan Anda telah menginstal **.NET SDK** (versi terbaru disarankan).
2.  Buka terminal/command prompt di folder project ini.
3.  Jalankan perintah berikut:
    ```bash
    dotnet run
    ```
4.  Atau, buka file `.csproj` menggunakan **Visual Studio** dan klik tombol Start/Run.

### Kontrol Aplikasi
Di panel sebelah kanan, terdapat slider untuk:
*   **Kecepatan Angin**: Mengatur kemiringan hujan.
*   **Gravitasi**: Mengatur kecepatan jatuh.
*   **Intensitas Hujan**: Mengatur kepadatan hujan.

---

## 🇬🇧 English

**RainSim** is an interactive rain simulation application built with C# and Windows Forms (.NET). It demonstrates simple particle animation with smooth performance (~60 FPS) and a parallax effect to create a sense of 3D depth.

Created with ❤️ by **Jacky The Code Bender**.

### Key Features
*   **Parallax Effect**: Raindrops have different "depths" (Z-depth). "Distant" drops appear smaller, more transparent, and fall slower than "near" drops.
*   **Wind Control**: Adjust wind direction and strength in real-time. Slide left for wind from the right, and vice versa.
*   **Gravity Control**: Adjust how fast the rain falls to the ground.
*   **Rain Intensity**: Control the number of rain particles, from a light drizzle (100 particles) to a heavy storm (3000 particles).
*   **Responsive**: The canvas adapts to the window size.

### How to Run
1.  Ensure you have the **.NET SDK** installed (latest version recommended).
2.  Open a terminal/command prompt in this project folder.
3.  Run the following command:
    ```bash
    dotnet run
    ```
4.  Alternatively, open the `.csproj` file using **Visual Studio** and click the Start/Run button.

### Controls
On the right panel, use the sliders to adjust:
*   **Wind Speed**: Controls the slant/direction of the rain.
*   **Gravity**: Controls the falling speed.
*   **Rain Intensity**: Controls the density of the rain.
