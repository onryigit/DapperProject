using DapperProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace DapperProject.Controllers;

[Route("Dashboard")]
public sealed class DashboardController(ITradeRepository repository) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await repository.GetDashboardAsync(cancellationToken));
}
