using DapperProject.Models;

namespace DapperProject.Services;

public interface ITradeRepository
{
    Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<TradeLog>> GetPagedAsync(int page, int pageSize, int? id, CancellationToken cancellationToken = default);
    Task<TradeLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TradeLog trade, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
