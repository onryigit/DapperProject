using System.ComponentModel.DataAnnotations;

namespace DapperProject.Models;

public sealed class TradeLog
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Kullanıcı Kodu")]
    public string UserCode { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Parite")]
    public string CryptoPair { get; set; } = string.Empty;

    [Required, RegularExpression("^(BUY|SELL)$")]
    [Display(Name = "İşlem Türü")]
    public string TradeType { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.00000001", "999999999999")]
    [Display(Name = "Fiyat")]
    public decimal Price { get; set; }

    [Range(typeof(decimal), "0.00000001", "999999999999")]
    [Display(Name = "Miktar")]
    public decimal Quantity { get; set; }

    [Display(Name = "Toplam USD")]
    public decimal TotalUSD { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    [Display(Name = "Komisyon USD")]
    public decimal FeeUSD { get; set; }

    [Required, StringLength(60)]
    [Display(Name = "Ülke")]
    public string LocationCountry { get; set; } = string.Empty;

    [Range(1, 10000)]
    [Display(Name = "İşlem Süresi (ms)")]
    public int ExecutionTimeMs { get; set; }

    [Display(Name = "İşlem Tarihi")]
    public DateTime TransactionDate { get; set; }
}
