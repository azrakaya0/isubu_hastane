# İSUBÜ Hastane Portalı

Hastane yönetim API’si (.NET 8) ve MAUI masaüstü/mobil istemcisi.

## Gereksinimler

- [Git](https://git-scm.com/download/win)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [.NET MAUI workload](https://learn.microsoft.com/dotnet/maui/get-started/installation) (Windows’ta istemci için)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (SQL Server için) **veya** SQL Server LocalDB
- Visual Studio 2022 (MAUI + ASP.NET iş yükleri) veya VS Code + terminal

## Projeyi bilgisayara alma (arkadaşınız için)

1. GitHub’daki repo sayfasında yeşil **Code** → **HTTPS** adresini kopyalayın.
2. Bir klasörde terminal (PowerShell) açın:

```powershell
git clone https://github.com/KULLANICI_ADINIZ/REPO_ADI.git
cd REPO_ADI
```

## Çalıştırma (Docker + API + istemci)

### 1) Veritabanı (Docker — önerilen)

Proje kök klasöründe:

```powershell
docker compose up -d
```

İlk seferde SQL Server imajı indirilir; birkaç dakika sürebilir.

### 2) API

Yeni bir terminal:

```powershell
cd src\HospitalApi
dotnet run --launch-profile http
```

API adresi: `http://localhost:5059`  
İlk çalıştırmada veritabanı migration ve demo verileri oluşturulur.

**LocalDB kullanmak isterseniz** (Docker yok):

```powershell
dotnet run --launch-profile http-localdb
```

### 3) MAUI istemci (Windows)

Başka bir terminal, proje kökünden:

```powershell
dotnet build src\Hospital.Client\Hospital.Client.csproj -f net8.0-windows10.0.19041.0
dotnet run --project src\Hospital.Client\Hospital.Client.csproj -f net8.0-windows10.0.19041.0
```

Visual Studio ile: `Hospital.sln` açın → **Hospital.Client** başlangıç projesi → **Windows Machine** hedefi → F5.

> API çalışmadan istemci giriş yapamaz. Önce API’yi başlatın.

## Demo giriş bilgileri

| Rol | Kullanıcı | Şifre |
|-----|-----------|-------|
| Admin | `admin` | `Admin123!` |
| Doktor | `dr.ayseyilmaz` (veya listedeki diğer `dr.*` kullanıcılar) | `Doctor123!` |
| Hasta | `hasta.demo` | `Patient123!` |

## Sorun giderme

- **API veritabanına bağlanamıyor:** Docker’ın çalıştığından emin olun (`docker compose ps`). `appsettings.Development.json` içindeki bağlantı dizesi Docker ile uyumludur.
- **Port 1433 meşgul:** Bilgisayarda başka SQL Server varsa Docker portunu değiştirin veya LocalDB profilini kullanın.
- **MAUI derlenmiyor:** `dotnet workload install maui` komutunu çalıştırın.

## Proje yapısı

- `src/HospitalApi` — REST API
- `src/Hospital.Client` — .NET MAUI uygulaması
- `src/Hospital.Shared` — Paylaşılan DTO’lar
- `docker-compose.yml` — Geliştirme SQL Server
