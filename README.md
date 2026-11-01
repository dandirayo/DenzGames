# DenzGames

DenzGames adalah monorepo dan launcher Windows untuk lima game Unity karya Dandi Prayogatama. Launcher membaca katalog dari JSON, memeriksa prasyarat Windows, mengekstrak build yang masih berupa ZIP, lalu menjalankan game dari satu antarmuka.

## Game

| Game | Engine | Build Windows | Lokasi source |
|---|---|---|---|
| Tower Defense | Unity 2018.3.3 | ZIP, diekstrak launcher saat pertama dimainkan | `games/Tower-Defense` |
| Survival Shooter | Unity 2018.3.3f1 | Siap dimainkan | `games/Survival-Shooter` |
| PONG | Unity 2018.3.3f1 | Siap dimainkan | `games/PONG` |
| Endless Runner | Unity 2019.4.41f2 | Siap dimainkan (build lama menurut repo asal) | `games/Endless-Runner` |
| Angry Birds Game | Unity 2018.3.3f1 | Siap dimainkan | `games/AngryBirdsGame` |

Setiap folder game diimpor dengan `git subtree`, sehingga riwayat repository asal tetap tersedia di monorepo ini.

## Menjalankan launcher

Prasyarat pengembangan: Windows 10/11 dan .NET 8 SDK atau lebih baru.

```powershell
dotnet build .\DenzGames.sln
dotnet run --project .\launcher\DenzGames.Launcher\DenzGames.Launcher.csproj
```

Launcher menemukan root repo secara otomatis melalui folder `manifest`. Build hasil ekstraksi disimpan di `%LOCALAPPDATA%\DenzGames\games` agar source repo tidak berubah.

## Struktur

```text
DenzGames/
├─ launcher/DenzGames.Launcher/  # aplikasi WPF .NET 8
├─ games/                        # lima repository game (git subtree)
├─ manifest/
│  ├─ games.json                 # katalog dan lokasi executable
│  └─ prerequisites.json         # aturan pemeriksaan prasyarat
├─ scripts/publish.ps1           # paket self-contained Windows x64
└─ DenzGames.sln
```

## Publish launcher

```powershell
.\scripts\publish.ps1
```

Hasil self-contained dan file SHA256 dibuat di `artifacts/`. Game tidak disalin ke paket launcher; pada rilis berikutnya, `downloadUrl` di manifest dapat diarahkan ke aset GitHub Releases agar game diunduh on-demand.

## Catatan distribusi

- Pastikan lisensi seluruh aset game mengizinkan distribusi ulang.
- File ZIP dan Unity package dikelola melalui Git LFS.
- Installer tanpa code-signing dapat memunculkan peringatan Windows SmartScreen.
- Jangan menghapus repository game lama sampai hasil migrasi dan rilis DenzGames sudah diverifikasi.

