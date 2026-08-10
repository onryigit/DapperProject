# TradePulse

Kripto ve borsa işlemleri için ASP.NET Core MVC, Dapper ve SQL Server tabanlı 1 milyon kayıtlık Big Data analiz paneli.

## Çalıştırma

1. SQL Server varsayılan yerel örneğinin çalıştığından emin olun.
2. Gerekirse `DapperProject/appsettings.json` içindeki `TradePulse` bağlantı dizesini kendi sunucunuza göre değiştirin.
3. Proje kökünde `dotnet run --project DapperProject/TradePulse.csproj` komutunu çalıştırın.

İlk çalıştırmada uygulama `TradePulseDb` veritabanını ve şemayı oluşturur; deterministik ve tutarlı tam 1.000.000 `TradeLog` kaydını 50.000'lik partilerle `SqlBulkCopy` üzerinden yükler ve performans indekslerini kurar. `SeedHistory` kaydı sayesinde CRUD işlemlerinden sonra uygulama yeniden başladığında veri seti tekrar oluşturulmaz.

## Rotalar

- `/Dashboard`: İstatistikler, ApexCharts grafikleri, basınç/kapasite göstergeleri, Top 5 işlemler ve Leaflet dünya haritası.
- `/Trades`: Dapper `OFFSET/FETCH` ile 20/50 kayıtlık server-side paging, ID sorgusu, modal güncelleme ve onaylı silme.

Dashboard toplu verileri tek bağlantı ve tek `QueryMultiple()` çağrısında alınır. Veri erişiminde Entity Framework kullanılmaz; yalnızca Dapper ve seeding için `SqlBulkCopy` vardır. Bağımsız SQL şeması `DapperProject/Database/Scripts/TradePulse.sql` dosyasındadır.
