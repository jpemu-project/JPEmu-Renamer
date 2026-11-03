// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

// Panduan（Bahasa Indonesia）。書式は HelpContent を参照
internal static class HelpId
{
    public const string Play = """
# Memulai
JPEmu Renamer merapikan ROM hasil dump Anda sendiri dan memungkinkan Anda langsung memainkannya – dengan emulator bawaan (core bawaan) atau emulator Anda sendiri (exe).

## Dari pengaturan sampai bermain
- Awalnya belum ada sistem. Gunakan "+ Tambah sistem" di kiri bawah untuk membuat sistem dengan nama apa pun (mis. Famicom, NES)
- Pilih sistem itu lalu atur folder ROM-nya di "Pengaturan sistem"
- Di tab "Main", pilih sebuah game
- Tekan "▶ Mainkan". Jika core belum terpasang, Anda akan ditanya apakah ingin mengunduhnya
- Tekan `Esc` untuk kembali ke peluncur

> Gunakan ROM yang Anda dump dari software milik Anda sendiri.

# Pengaturan sistem
Pilih sistem di kiri lalu buka "Pengaturan sistem" di kanan atas. Tambahkan sistem dengan "+ Tambah sistem". Memilih templat akan mengisi ekstensi, ikon, warna, dan core bawaan.

| Item | Keterangan |
| Nama sistem | Nama yang tampil di daftar |
| Ikon / warna | Ikon di daftar sistem. Gambar sendiri (PNG/ICO/JPG) juga bisa |
| File DAT | Daftar game untuk penggantian nama (format OfflineList). Tidak perlu jika hanya bermain |
| Folder ROM | Folder berisi ROM. Pisahkan dengan `;` jika lebih dari satu |
| Ekstensi | Ekstensi yang dibaca (mis. `.nes;.zip`) |
| Cara menjalankan bawaan | Core bawaan atau emulator |
| Core bawaan | Core untuk sistem ini. Dapatkan dengan "Unduh" / "Perbarui" |
| Emulator / argumen | Dipakai saat bermain dengan emulator eksternal |

## Sistem yang butuh BIOS
Famicom Disk System, Mega-CD, PC Engine CD-ROM², Saturn, Neo Geo, dan lainnya membutuhkan file BIOS. Nama file ditampilkan di bawah core bawaan pada "Pengaturan sistem". Taruh file itu di "Pengaturan permainan" → "Lanjutan" → "Folder BIOS" (bawaan: `system` di folder pengaturan).

# Menjalankan game
## Cara menjalankan
- Pilih game lalu tekan "▶ Mainkan"
- Klik dua kali sebuah game
- Pilih game lalu tekan `Enter`

## Mengganti cara menjalankan
Klik kanan sebuah game lalu pilih "Jalankan dengan core bawaan" atau "Jalankan dengan emulator (exe)". Cara bawaan diatur di "Pengaturan sistem".

## Argumen emulator
| Penanda | Diganti dengan |
| `{rom}` | Jalur lengkap ROM |
| `{romdir}` | Folder ROM |
| `{romname}` | Nama file ROM tanpa ekstensi |

# Kontrol
## Tata letak keyboard bawaan
Nama tombol mengikuti tata letak Super Famicom (A di kanan, B di bawah).

| Kontrol | Tombol |
| Tombol arah | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Shift kanan` / `Enter` |

## Kontroler
Kontroler yang terhubung dikenali otomatis dan diberi tata letak standar: Xbox (XInput), kontroler USB umum (DirectInput), Switch Pro Controller, DualSense, dan lainnya. Pemain 1 memakai kontroler yang pertama terhubung, pemain 2 yang kedua. Stik kiri juga berfungsi sebagai tombol arah. Getaran berfungsi pada game yang mendukungnya.

## Pintasan
| Aksi | Tombol | Kontroler |
| Menu | `F1` | Tombol Guide |
| Percepat (tahan) | `Tab` | |
| Simpan state | `F2` | |
| Muat state | `F4` | |
| Slot − / + | `F6` / `F7` | |
| Tangkapan layar | `F12` | |
| Layar penuh hidup/mati | `F11` atau `Alt+Enter` | |
| Jeda | `P` | |
| Reset | `F10` | |
| Keluar | `Esc` | |

> Semua tombol bisa diubah di "Pengaturan permainan". Saat layar penuh, `Esc` kembali ke jendela terlebih dahulu.

# Menu dalam game
`F1` (atau tombol Guide di kontroler) membuka menu dan menjeda game. Gunakan `↑` `↓` untuk memilih, `Enter` (A) untuk konfirmasi, dan `Esc` (B) untuk menutup.

| Item | Keterangan |
| Lanjutkan | Menutup menu |
| Simpan / muat state | Di slot saat ini |
| Ganti slot | Berpindah slot |
| Tangkapan layar | Menyimpan layar game sebagai PNG |
| Layar penuh / jendela | Mengganti tampilan |
| Pengaturan permainan… | Mengubah pengaturan saat bermain; langsung berlaku |
| Reset | Memulai ulang game |
| Keluar ke peluncur | Mengakhiri game |

# Data simpanan dan save state
## Data simpanan (baterai)
Simpanan yang Anda buat di dalam game disimpan otomatis (sekitar setiap 10 detik dan saat keluar) dan dimuat lagi lain kali.

## Save state
Menyimpan saat persis di dalam game. `F2` menyimpan, `F4` memuat, `F6` / `F7` mengganti slot (bawaan 10 slot, 0–9).

## Lokasi
| Jenis | Lokasi bawaan |
| Data simpanan | `saves\<sistem>` di folder pengaturan |
| Save state | `states\<sistem>` di folder pengaturan |
| Tangkapan layar | `screenshots\<sistem>` di folder pengaturan |

Folder pengaturan adalah `%APPDATA%\JPEmuRenamer` (atau `data` di sebelah exe jika ada `portable.txt`). Lokasi bisa diubah di "Pengaturan permainan".

# Pengaturan permainan
Buka lewat tombol "Pengaturan permainan" di tab Main, klik kanan pada game, menu dalam game, atau "Pengaturan" aplikasi.

## Semua, sistem, dan game
Pengaturan bertumpuk dengan urutan "Semua → Sistem → Game". Pilih di atas level yang ingin diubah. Level yang lebih tinggi bisa menggantikan nilai level di bawahnya. Item yang digantikan ditandai ●, dan ↺ membatalkannya. Contoh: jendela untuk semua, layar penuh hanya untuk satu game.

## Tab "Dasar"
| Kategori | Keterangan |
| Kontroler & keyboard | Kontrol pemain 1. Klik item lalu tekan tombol (`Esc` membatalkan, `Delete` menghapus) |
| Lokasi penyimpanan | Folder data simpanan, save state, dan tangkapan layar |
| Video | Jendela/layar penuh, ukuran jendela (1x/2x/3x/sesuaikan), rasio aspek |
| Audio | Volume, bisukan, perangkat keluaran |

## Tab "Lanjutan"
| Kategori | Keterangan |
| Kontroler & keyboard | Pemain 1–4, turbo dan kecepatannya, zona mati stik, pintasan. Kombinasi tombol (mis. `Alt+Enter`) didukung |
| Lokasi penyimpanan | Subfolder (tidak ada/per sistem/per game), simpan di sebelah ROM, jumlah slot, folder BIOS |
| Video | Resolusi dan refresh rate layar penuh, skala bilangan bulat, pemfilteran, shader (garis pindai/CRT), V-Sync |
| Audio | Sample rate, mode keluaran (WASAPI bersama/eksklusif), latensi, koreksi sinkron audio |
| Opsi core | Opsi milik core (mis. palet NES, rasio aspek, overclock). Muncul setelah game dijalankan sekali dengan core itu |

"Setel ulang tab ini" mengembalikan tab ke nilai bawaan untuk level yang dipilih.

# Pratinjau otomatis
Di tab "Main", pilih game lalu tunggu sebentar – cuplikan game mulai diputar di kanan atas. Core bawaan berjalan tanpa suara dan input, dan menampilkan demo setelah layar judul. Core bawaan sistem harus sudah terpasang.

| Item | Bawaan |
| Mulai | 0,3 detik setelah dipilih (tidak saat menggulir cepat) |
| Kecepatan | 4x |
| Lewati awal | 10 detik pertama (logo, dll.) |
| Ulang setelah | 60 detik |
| Suara / input | Tidak ada (data simpanan tidak diubah) |

Ubah nilai ini, atau matikan pratinjau, di "Pengaturan" → "Pratinjau otomatis".

## Cuplikan
- Saat pratinjau, "Simpan layar ini" di kanan bawah menyimpan saat itu sebagai cuplikan
- Klik kanan game → "Buat cuplikan untuk sistem ini…" menjalankan setiap ROM sistem di latar belakang dan menyimpan layar pada waktu tertentu (bawaan 30 detik). Game yang sudah punya cuplikan bisa dilewati
- Cuplikan ditampilkan sebelum pratinjau dimulai dan pada game yang layarnya tidak bergerak
- Disimpan sebagai `snapshots\<sistem>\<judul>.png` di folder pengaturan

> Menjalankan game saat pratinjau atau pembuatan cuplikan akan menghentikan pratinjau terlebih dahulu.

# Mengunduh dan memperbarui core
Buka "Pengaturan" → "Unduh / perbarui core bawaan".

- Daftar menampilkan sistem, ukuran, dan status setiap core (belum terpasang / terbaru / ada pembaruan)
- Pilih core dengan "Core sistem saya", "Belum terpasang", "Ada pembaruan", dll., lalu tekan "Unduh"
- "Perbarui semua" memperbarui semua core yang punya pembaruan

> Core diunduh dari situs resmi libretro. Core yang dipakai game yang sedang berjalan tidak bisa diperbarui.

# Pemecahan masalah
| Masalah | Yang perlu diperiksa |
| Tidak mau jalan / "tidak dapat memuat game" | Jika sistem butuh BIOS, periksa folder BIOS. Core lain mungkin berhasil |
| "OpenGL tidak tersedia" | Core memakai OpenGL (mis. N64). Perbarui driver grafis atau pilih core lain |
| Suara pecah atau tertinggal | Naikkan latensi atau aktifkan koreksi sinkron audio di "Pengaturan permainan" → "Lanjutan" → "Audio" |
| Gerakan patah-patah | Matikan V-Sync atau tutup aplikasi lain. Core berat seperti N64 butuh PC yang cepat |
| Kontroler tidak merespons | Kontroler yang dikenali tercantum di bagian atas "Pengaturan permainan". Sambungkan ulang jika tidak ada. Kontroler pertama adalah pemain 1 |
| Resolusi tidak kembali | Keluar dari game atau tekan `Esc` untuk kembali ke jendela |
| Tekanan pertama diabaikan | Beberapa game tidak menerima input selama beberapa detik setelah mulai. Tunggu sebentar |
| Tidak ada pratinjau otomatis | Pastikan core bawaan sistem terpasang dan pratinjau otomatis aktif di "Pengaturan". Pratinjau dijeda saat aplikasi di latar belakang |
""";

    public const string Rename = """
# Cara kerja penggantian nama
Aplikasi menghitung CRC32 (sidik jari isi file) setiap ROM dan membandingkannya dengan file DAT (daftar judul dan CRC32). Hanya file yang cocok yang diganti namanya menjadi nama yang benar dari DAT, apa pun namanya sekarang.

> Anda selalu memeriksa daftar sebelum mengganti nama, dan "Batalkan" mengembalikan seluruh proses setelahnya.

# Persiapan
## Mendapatkan file DAT
Gunakan DAT berformat OfflineList (XML) untuk setiap sistem. DAT lama berkode Shift-JIS juga didukung.

## Mengatur sistem
Pilih sistem di kiri lalu atur hal berikut di "Pengaturan sistem".

| Item | Keterangan |
| File DAT | File DAT |
| Folder ROM | Folder berisi ROM (pisahkan dengan `;`) |
| Ekstensi | Ekstensi yang dibaca (mis. `.nes;.zip`) |
| Penanganan header | Apakah header iNES/FDS ikut dihitung di CRC32 (biasanya "Otomatis") |

Pembacaan dan perbandingan dimulai otomatis. Mulai kali kedua, file yang tidak berubah dilewati sehingga cepat.

# Tab "Ganti nama"
Beralih ke tab "Ganti nama" di atas untuk melihat tabel.

| Kolom | Keterangan |
| ✓ | Apakah diganti namanya (hanya baris "Perlu diganti" yang bisa dicentang) |
| Status | Hasil perbandingan (lihat di bawah) |
| Nama file saat ini | Nama sekarang |
| → Nama di DAT | Nama setelah diganti |
| CRC32 / ukuran / format | Informasi ROM |

## Status
| Status | Arti | Diganti namanya |
| Cocok | CRC32 cocok dan nama sudah benar | Tidak perlu |
| Perlu diganti | CRC32 cocok tetapi namanya berbeda | Ya |
| Duplikat | Sudah ada file untuk judul yang sama | Tidak |
| Tidak ada di DAT | CRC32 tidak ada di DAT | Tidak |
| Belum punya | Ada di DAT, tetapi Anda tidak punya filenya | ― |

# Mengganti nama langkah demi langkah
- Di tab "Ganti nama", periksa baris "Perlu diganti" (awalnya semua dicentang)
- Hapus centang pada yang tidak ingin diganti. Kotak di judul kolom mengubah semuanya
- Di kanan tampil "Sebelum" dan "Sesudah" untuk baris yang dipilih
- Tekan "Ganti nama" di bawah lalu konfirmasi dengan "Ya"
- Jumlahnya ditampilkan dan daftar diperbarui sendiri

> Hanya file yang CRC32-nya cocok dengan DAT yang diubah. File dilewati jika file dengan nama baru sudah ada.

## Nama ROM di dalam ZIP
Jika "Pengaturan" → "Ganti juga nama ROM di dalam file ZIP" aktif, ROM di dalam setiap ZIP diganti namanya bersama ZIP-nya.

## Karakter yang tidak diizinkan Windows
`\` `/` `:` `*` `?` `"` `<` `>` `|` diganti dengan karakter lebar penuh.

# Batalkan
"Batalkan" di bawah tab "Ganti nama" mengembalikan seluruh proses terakhir. Tekan lagi untuk mengembalikan proses sebelumnya (hingga 50 terakhir).

> "Batalkan" selalu mengembalikan proses terbaru, apa pun sistemnya. Konfirmasi menampilkan tanggal dan jumlah filenya.

# Fitur praktis
| Fitur | Cara pakai |
| Dimiliki / Semua / Belum punya | Menyaring daftar. "Belum punya" menampilkan game yang belum Anda miliki |
| Cari | Ketik di kotak pencarian untuk menyaring berdasarkan judul atau nama file |
| Bandingkan ulang | Gunakan setelah menambah atau menghapus ROM |
| Sembunyikan ekstensi | "Pengaturan" → "Sembunyikan ekstensi file (.zip dll.) di daftar judul" |
| Baca subfolder | "Pengaturan" → "Baca juga subfolder" |
| Hanya tampilkan nama DAT | Untuk arkade, yang emulatornya tidak jalan jika nama file diubah. Aktifkan di "Pengaturan sistem" untuk mempertahankan nama file dan menampilkan nama DAT di daftar |

# Pemecahan masalah
| Masalah | Yang perlu diperiksa |
| Tidak ada yang cocok | Pastikan DAT untuk sistem yang benar. Mengubah "Penanganan header" ke "Dengan header" atau "Tanpa header" bisa membantu |
| "File DAT tidak dapat dibaca" | Pastikan itu DAT OfflineList (XML) |
| "Duplikat" | Anda punya dua ROM atau lebih untuk judul yang sama. Pindahkan salah satunya ke folder lain |
| Dilewati dengan "(sudah ada)" | File dengan nama baru sudah ada. Tentukan mana yang disimpan lalu rapikan secara manual |
| Nama di dalam ZIP tidak berubah | Jika nama ZIP-nya sudah benar, nama di dalamnya dibiarkan |
""";
}
