using DapperProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace DapperProject.Controllers;

[Route("Dashboard")]
public sealed class DashboardController(ITradeRepository repository) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            return View(await repository.GetDashboardAsync(cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
    }
}
