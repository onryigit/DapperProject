using DapperProject.Models;
using DapperProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace DapperProject.Controllers;

[Route("Trades")]
public sealed class TradesController(ITradeRepository repository) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, int? id = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = pageSize is 20 or 50 ? pageSize : 20;
        ViewBag.SearchId = id;
        return View(await repository.GetPagedAsync(page, pageSize, id, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var trade = await repository.GetByIdAsync(id, cancellationToken);
        return trade is null ? NotFound(new { message = $"#{id} numaralı işlem bulunamadı." }) : Ok(trade);
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(TradeLog trade, int returnPage = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Kayıt güncellenemedi. Alanları kontrol edin.";
            return RedirectToAction(nameof(Index), new { page = returnPage, pageSize });
        }

        var updated = await repository.UpdateAsync(trade, cancellationToken);
        TempData[updated ? "Success" : "Error"] = updated
            ? $"#{trade.Id} numaralı işlem güncellendi."
            : "İşlem bulunamadı.";
        return RedirectToAction(nameof(Index), new { page = returnPage, pageSize });
    }

    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int returnPage = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        TempData[deleted ? "Success" : "Error"] = deleted
            ? $"#{id} numaralı işlem silindi."
            : "Silinecek işlem bulunamadı.";
        return RedirectToAction(nameof(Index), new { page = returnPage, pageSize });
    }
}
