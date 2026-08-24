# 📈 TradePulse

TradePulse; kripto para işlem verilerini analiz etmek ve yönetmek için geliştirilmiş, **ASP.NET Core MVC**, **Dapper** ve **SQL Server** tabanlı bir Big Data dashboard uygulamasıdır. Proje, varsayılan kurulumda üretilen **1.000.000 anlamlı işlem kaydı** üzerinde istatistik üretme, görselleştirme, sunucu taraflı sayfalama ve CRUD operasyonlarını yüksek performansla gerçekleştirme yaklaşımını gösterir.

## Öne Çıkan Özellikler

- Toplam işlem hacmi, komisyon, en yüksek hacimli parite ve ortalama işlem süresi istatistikleri
- Veri setinin son 30 gününe ait hacim trendi ve önceki dönem karşılaştırmaları
- Parite ve alış/satış dağılımlarını gösteren interaktif ApexCharts grafikleri
- Ülke bazlı işlem yoğunluklarını gösteren Leaflet dünya haritası
- En yüksek hacimli işlemler için hızlı özet tablosu
- 1 milyon kayıt üzerinde SQL Server tarafında çalışan sayfalama
- Birincil anahtar üzerinden ID bazlı hızlı kayıt sorgulama
- Doğrulamalı güncelleme ve onay ekranına sahip silme işlemleri
- Dapper parametreleri ve anti-forgery token ile güvenli veri işlemleri
- İlk çalıştırmada otomatik veritabanı, şema, indeks ve veri seti oluşturma
- Masaüstü, tablet ve mobil ekranlara uyumlu responsive arayüz

## Kullanılan Teknolojiler

| Katman | Teknoloji |
|---|---|
| Backend | .NET 10, ASP.NET Core MVC, C# |
| Veri erişimi | Dapper 2.1, Microsoft.Data.SqlClient |
| Veritabanı | Microsoft SQL Server |
| Büyük veri yükleme | SqlBulkCopy |
| Arayüz | Razor Views, Bootstrap, özel CSS |
| Grafikler | ApexCharts |
| Harita | Leaflet, Carto basemap |
| İstemci tarafı | Vanilla JavaScript |

## Proje Mimarisi ve Veri Akışı

Uygulama; sunum, uygulama/veri erişimi ve veritabanı sorumluluklarını birbirinden ayıran katmanlı bir yapı kullanır. Controller sınıfları HTTP akışını yönetirken bütün SQL işlemleri repository üzerinden gerçekleştirilir.

```mermaid
flowchart LR
    subgraph Client["İstemci Katmanı"]
        Browser["Web Tarayıcısı"]
        Dashboard["Dashboard"]
        Trades["Veri Tablosu"]
        Visuals["ApexCharts ve Leaflet"]
    end

    subgraph Presentation["ASP.NET Core MVC"]
        Routing["Routing ve Middleware"]
        DashboardController["DashboardController"]
        TradesController["TradesController"]
        Validation["Model Validation ve Anti-forgery"]
        Views["Razor Views"]
    end

    subgraph DataAccess["Veri Erişim Katmanı"]
        Contract["ITradeRepository"]
        Repository["TradeRepository"]
        Context["DapperContext"]
        Seeder["DatabaseSeeder"]
    end

    subgraph SqlServer["SQL Server"]
        Connection["Microsoft.Data.SqlClient"]
        TradeLogs[("TradeLogs - 1M Kayıt")]
        SeedHistory[("SeedHistory")]
        Indexes["Performans İndeksleri"]
    end

    Browser -->|"HTTP isteği"| Routing
    Routing -->|"GET /Dashboard"| DashboardController
    Routing -->|"GET ve POST /Trades"| TradesController
    TradesController --> Validation
    DashboardController --> Contract
    Validation --> Contract
    Contract --> Repository
    Repository --> Context
    Context --> Connection
    Connection -->|"Parametreli Dapper sorguları"| TradeLogs
    TradeLogs --- Indexes

    Repository -->|"QueryMultiple sonuçları"| DashboardController
    Repository -->|"Paging, ID ve CRUD sonuçları"| TradesController
    DashboardController --> Views
    TradesController --> Views
    Views --> Dashboard
    Views --> Trades
    Dashboard --> Visuals
    Dashboard --> Browser
    Trades --> Browser

    Seeder -->|"İlk çalıştırma kontrolü"| SeedHistory
    Seeder -->|"50.000 satırlık SqlBulkCopy partileri"| TradeLogs
```

## Veri Seti

Veri seti, 85.000 kullanıcı kodundan seçilen kripto para alım ve satım işlemlerini temsil eder.

| Alan | Açıklama |
|---|---|
| `Id` | Benzersiz işlem kimliği |
| `UserCode` | `USR-000001` formatında kullanıcı kodu |
| `CryptoPair` | BTC, ETH, BNB, SOL, XRP, ADA, AVAX veya DOGE / USDT paritesi |
| `TradeType` | BUY veya SELL işlem yönü |
| `Price` | İşlem anındaki birim fiyat |
| `Quantity` | Alınan veya satılan varlık miktarı |
| `TotalUSD` | Fiyat ve miktardan hesaplanan işlem toplamı |
| `FeeUSD` | İşlem toplamından hesaplanan komisyon |
| `LocationCountry` | İşlemin gerçekleştiği ülke |
| `ExecutionTimeMs` | Milisaniye cinsinden işlem süresi |
| `TransactionDate` | UTC işlem tarihi |

Veriler sabit bir random seed ile anlamlı fiyat aralıklarında üretilir. `TotalUSD` ve `FeeUSD` alanları diğer finansal alanlardan hesaplandığı için kayıtlar kendi içinde tutarlıdır.

## Performans Yaklaşımı

- **Toplu yükleme:** 1 milyon kayıt, tek tek `INSERT` yerine `SqlBulkCopy` ile yüklenir.
- **Partili üretim:** Bellek kullanımını kontrol altında tutmak için 50.000 satırlık partiler kullanılır.
- **Sunucu taraflı paging:** Uygulama yalnızca ekranda gösterilecek 20 veya 50 kaydı SQL Server'dan alır.
- **Tek bağlantıda dashboard:** Bağımsız dashboard sorguları `QueryMultiple()` ile tek round-trip içinde çalıştırılır.
- **Parametreli sorgular:** Bütün filtre ve CRUD değerleri Dapper parametreleriyle SQL Server'a gönderilir.
- **İndeksler:** Tarih, parite, işlem türü, toplam tutar ve ülke sorguları için covering index yapıları bulunur.
- **Asenkron I/O:** Veritabanı operasyonları async olarak çalışır ve `CancellationToken` destekler.

## Proje Yapısı

```text
DapperProject/
├── Context/
│   └── DapperContext.cs          # SQL bağlantılarının oluşturulması
├── Controllers/
│   ├── DashboardController.cs    # Dashboard HTTP akışı
│   ├── TradesController.cs       # Paging, ID sorgusu ve CRUD akışı
│   └── HomeController.cs         # Ana yönlendirme ve hata sayfası
├── Data/
│   └── DatabaseSeeder.cs         # Şema ve 1 milyon kayıt üretimi
├── Database/Scripts/
│   └── TradePulse.sql            # Bağımsız, idempotent SQL şeması
├── Models/
│   ├── DashboardViewModel.cs     # Dashboard sonuç modelleri
│   ├── PagedResult.cs            # Sayfalama modeli
│   └── TradeLog.cs               # İşlem ve doğrulama modeli
├── Services/
│   ├── ITradeRepository.cs       # Veri erişim sözleşmesi
│   └── TradeRepository.cs        # Dapper sorguları ve CRUD işlemleri
├── Views/
│   ├── Dashboard/                # Dashboard Razor görünümü
│   ├── Trades/                   # Veri tablosu ve düzenleme arayüzü
│   └── Shared/                   # Ortak layout ve hata görünümü
├── wwwroot/
│   ├── css/                      # Responsive uygulama tasarımı
│   └── js/                       # Dashboard, tablo ve ortak etkileşimler
├── appsettings.json              # Bağlantı ve seed ayarları
└── Program.cs                    # DI, middleware ve başlangıç akışı
```

## Uygulama Rotaları

| Metot | Rota | Açıklama |
|---|---|---|
| GET | `/` | Dashboard sayfasına yönlendirir |
| GET | `/Dashboard` | İstatistikleri, grafikleri, haritayı ve özet tabloyu getirir |
| GET | `/Trades` | Sunucu tarafında sayfalanmış işlem tablosunu getirir |
| GET | `/Trades/{id}` | ID'ye göre işlem detayını JSON olarak getirir |
| POST | `/Trades/Update` | İşlem kaydını doğrular ve günceller |
| POST | `/Trades/Delete/{id}` | Onaylanan işlem kaydını siler |

## Teknik Kazanımlar

- Dapper ve repository pattern ile sürdürülebilir veri erişim katmanı oluşturma
- `SqlBulkCopy` kullanarak büyük veri setlerini performanslı biçimde yükleme
- SQL Server için sorgu, covering index ve server-side paging tasarlama
- `QueryMultiple()` ile birden fazla dashboard sonucunu tek round-trip içinde işleme
- Parametreli sorgularla ID sorgulama, güncelleme ve silme operasyonları geliştirme
- ASP.NET Core MVC model binding, validation ve anti-forgery mekanizmalarını uygulama
- Razor üzerinden backend verisini JavaScript grafiklerine ve harita bileşenlerine aktarma
- Asenkron veritabanı işlemleri ve istek iptali yönetimi
- Responsive ve veri odaklı bir dashboard arayüzü geliştirme


<img width="1895" height="945" alt="a111111" src="https://github.com/user-attachments/assets/761f97f1-2a8c-4efd-9886-a6d355e21ae5" />
<img width="1901" height="951" alt="a6" src="https://github.com/user-attachments/assets/05957fab-969d-4269-9696-627a2dd37742" />
<img width="1901" height="946" alt="a5" src="https://github.com/user-attachments/assets/75e41391-c9b8-4645-a433-e3871e9f160a" />
<img width="1900" height="940" alt="a4" src="https://github.com/user-attachments/assets/ee7e0cee-9449-40b4-99ef-edd4a3c39206" />
<img width="1903" height="950" alt="a3" src="https://github.com/user-attachments/assets/b3c8011b-d1ed-4f31-aae0-c92097d35acf" />
<img width="1902" height="946" alt="a2" src="https://github.com/user-attachments/assets/08a87d54-9c88-4398-8734-44ea667074ec" />
<img width="1904" height="944" alt="a1" src="https://github.com/user-attachments/assets/7bea1dc9-db0b-495f-8f33-08c643e1fd5d" />
<img width="1903" height="944" alt="a00" src="https://github.com/user-attachments/assets/fa25d37f-766d-4ddd-abb8-5a4bb698424c" />
<img width="1903" height="952" alt="a0" src="https://github.com/user-attachments/assets/dd9761d1-813c-4fce-8b34-6206c6188b9c" />


