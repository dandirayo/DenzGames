# Panduan Setup Fitur Baru (Unity Editor)

File `endlessrunner_fileproject.zip` berisi project yang sudah di-update:
efisiensi kode (sudah otomatis jalan, tidak perlu setup apa-apa) + 6 script baru untuk fitur
yang perlu di-wiring manual di Unity Editor (scene/prefab/UI tidak aman diedit lewat teks,
jadi bagian ini perlu kamu lakukan sendiri di Editor).

## 0. Yang Sudah Otomatis Jalan (tidak perlu setup)
- High score persist lewat `PlayerPrefs` (`ScoreData.cs`)
- Terrain generator pakai Queue/Stack, lebih efisien (`TerrainGeneratorController.cs`)
- UI skor cuma update teks kalau nilai berubah (`UIScoreController.cs`)
- Input lompat: mouse klik **atau** tombol Space, + jump buffer & coyote time biar lompat terasa lebih responsif (`CharacterMoveController.cs`)
- Difficulty scaling: karakter makin cepat seiring skor naik (field `speedIncreasePerStep`, `difficultyScoreStep`, `absoluteMaxSpeed` di Inspector `CharacterMoveController`)
- Double jump: field `extraJumps` di `CharacterMoveController` (default 1 = bisa lompat 2x, set 0 untuk balik ke single jump)
- Bug fix kecil: `TerrainGeneratorController.ReturnToPool` dulu bisa crash (NullReferenceException) kalau ada item pool yang belum terdaftar; sekarang `return` dengan aman setelah log error.

## 1. Main Menu
1. Buat scene baru: `Assets/Scenes/MainMenu.unity`
2. Tambah UI Canvas → Button "Play", Button "Quit", Text (opsional) untuk tampilkan high score
3. Buat empty GameObject `MainMenuManager`, attach script **`MainMenuController.cs`**
4. Isi field `gameplaySceneName` = `EndlessRunnerTheGame`
5. Hubungkan tombol:
   - Play → `MainMenuManager.PlayGame()`
   - Quit → `MainMenuManager.QuitGame()`
6. Tambahkan kedua scene (`MainMenu`, `EndlessRunnerTheGame`) ke **File > Build Settings**, urutan: MainMenu di index 0

## 2. Pause Menu
1. Di scene `EndlessRunnerTheGame`, buat empty GameObject `GameManager`, attach **`GameManager.cs`**
2. Buat Panel UI "Pause Panel" (nonaktifkan di awal), isi tombol Resume/Restart/Main Menu
3. Set `pausePanel` di `GameManager` ke Panel tsb
4. Buat GameObject lain (atau di Panel yang sama) attach **`PauseMenuController.cs`**, isi:
   - `gameManager` → drag GameObject `GameManager`
   - `mainMenuSceneName` = `MainMenu`
5. Hubungkan tombol: Resume/Restart/BackToMainMenu ke method yang sama namanya di `PauseMenuController`
6. Tekan **Esc** saat main = otomatis pause/resume (sudah built-in di `GameManager`)

## 3. Obstacle (rintangan aktif)
1. Pastikan GameObject `Main Character` punya **Tag = "Player"** (Inspector kiri atas)
2. Buat prefab rintangan baru (mis. duri/batu), tambah `Collider2D` (BoxCollider2D/PolygonCollider2D) dengan **Is Trigger = true**
3. Attach **`ObstacleController.cs`** ke prefab tsb
4. Taruh instance-instance obstacle di atas/dalam prefab `Ground Template 1/2/3`, atau buat prefab terrain baru khusus yang sudah menyertakan obstacle → daftarkan ke `terrainTemplates` di `TerrainGeneratorController`

## 4. Coin / Collectible
1. Buat prefab koin (sprite bulat kuning misalnya), `Collider2D` **Is Trigger = true**
2. Attach **`CoinController.cs`**, isi `bonusScore` (default 10), opsional `pickupSound`/`pickupVfx`
3. Taruh beberapa koin di prefab terrain (mirip cara pasang obstacle di atas)

## 5. Tema Indonesia (Lubang Jalan, Polisi Tidur, Razia Polisi)

**Catatan penting**: belum ada sprite/aset gambar bertema Indonesia di project ini —
tidak ada tool image-generation di environment saya untuk bikinkan PNG-nya. Yang
disiapkan di sini adalah **mekanik gameplay-nya**, dibangun pakai shape primitif
bawaan Unity (kotak/lingkaran + warna solid, sprite "Sprites/Square" & "Sprites/Knob"
yang sudah built-in di Unity, TANPA perlu import gambar apapun) supaya bisa langsung
dicoba di Editor. Kalau nanti sudah punya/dapat sprite asli (mis. dari Kenney.net,
itch.io aset CC0, atau gambar sendiri), tinggal drag-drop ganti `Sprite` di
`SpriteRenderer` masing-masing prefab — script & collision-nya tidak perlu diubah.

### 5a. Lubang Jalan (pothole) — pakai `ObstacleController.cs`
1. Buat GameObject baru "Lubang Jalan", tambah `SpriteRenderer` → sprite bawaan
   `Sprites/Square`(warna hitam/coklat tua), scale pipih (mis. X=1.2, Y=0.3), posisikan
   sedikit "tenggelam" di permukaan jalan
2. Tambah `Collider2D`, centang **Is Trigger**
3. Attach `ObstacleController.cs` → tabrak = game over (harus dilompati, sama seperti obstacle biasa)

### 5b. Polisi Tidur (speed bump) — pakai `SpeedBumpObstacle.cs` (baru)
1. Buat GameObject baru "Polisi Tidur", `SpriteRenderer` → `Sprites/Square` warna
   kuning-hitam (mis. `#F2C230`), scale pipih memanjang, kecil (gundukan)
2. `Collider2D` **Is Trigger**
3. Attach **`SpeedBumpObstacle.cs`** — beda dari pothole: **bukan** instant game over,
   tapi memperlambat karakter sesaat (`slowDuration`, `slowFactor` bisa diatur di Inspector).
   Lebih ramah & sesuai nuansa "polisi tidur" beneran (bikin pelan, bukan bikin celaka).

### 5c. Razia Polisi (pos/mobil patroli) — pakai `ObstacleController.cs` + `PoliceSirenController.cs` (baru)
1. Buat GameObject "Pos Razia", `SpriteRenderer` → `Sprites/Square` warna putih/biru gelap
   (badan mobil/pos), `Collider2D` **Is Trigger**, attach `ObstacleController.cs` (lethal, harus dilompati)
2. Tambah **child GameObject** kecil di atasnya (lampu sirine): `SpriteRenderer` →
   `Sprites/Knob` (lingkaran kecil) atau `Sprites/Square`, attach **`PoliceSirenController.cs`**
   → otomatis kedap-kedip merah-biru tanpa perlu animasi/sprite tambahan
3. (Opsional) atur `blinkSpeed` di Inspector untuk kecepatan kedip

### 5d. Variasi tambahan (ide "dan lain-lain", tinggal dipakai script yang sama)
Semua bisa dibuat cukup dengan `ObstacleController.cs` (lethal) atau
`SpeedBumpObstacle.cs` (slow-down) + warna/bentuk berbeda, tanpa script baru:
- **Angkot ngetem** di tengah jalan → `ObstacleController` (kotak kuning memanjang)
- **Portal proyek/galian** → `ObstacleController` (kotak oranye + garis diagonal warna)
- **Palang perlintasan kereta** → `ObstacleController` (kotak tipis merah-putih diagonal)

### 5e. Pasang ke jalur
Sama seperti langkah obstacle biasa (bagian 3) — taruh instance-instance di atas
prefab `Ground Template 1/2/3`, atau bikin varian prefab terrain baru yang sudah
menyertakan obstacle Indonesia ini, lalu daftarkan ke `terrainTemplates` di
`TerrainGeneratorController` biar muncul acak sepanjang track.

## 6. Musik & Tombol Mute
1. Buat GameObject `MusicPlayer`, attach `AudioSource` + **`MusicController.cs`**
2. Isi `backgroundMusic` dengan file musik (drag audio clip)
3. (Opsional) Tambah tombol "Sound" di UI, hubungkan ke `MusicPlayer.ToggleMute()`, dan drag Text tombol ke field `muteButtonLabel` biar teksnya otomatis berubah "Sound: On/Off"
4. Kalau mau musik lanjut dari Main Menu ke Gameplay, tambahkan `DontDestroyOnLoad(gameObject)` sederhana atau taruh `MusicPlayer` cuma di scene MainMenu dan biarkan gameplay tanpa BGM baru

## Catatan
- Semua field baru di Inspector punya default value yang aman — kalau tidak sempat setup fitur baru dulu, project tetap jalan seperti semula (cuma lebih efisien).
- `extraJumps = 0` di `CharacterMoveController` untuk menonaktifkan double jump kalau tidak diinginkan.
- `endlessrunner_fileproject.zip` sudah berisi versi terbaru (source project, hasil efisiensi + fitur baru). Untuk lihat perubahan versi ke versi, cek riwayat commit git.
- `endlessrunner_standalone/` **belum di-rebuild** dari kode terbaru — itu masih build lama (`.exe`) dari kode sebelum efisiensi & fitur baru. Untuk update, buka project di Unity Editor lalu **File > Build Settings > Build**, timpa folder tsb.
