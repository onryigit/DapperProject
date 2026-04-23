using DapperProject.Dtos.CustomerDtos;
using System.Runtime.InteropServices;

namespace DapperProject.Services.CustomerServices
{
    public interface ICustomerService
    {
        Task<List<ResultCustomerDto>> GetAllCustomerAsync();
        Task<GetCustomerByIdDto> GetCustomerByIdAsync(int id);
        Task CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        Task UpdateCustomerAsync(UpdateCustomerDto updateCustomerDto);
        Task DeleteCustomerAsync(int id);
    }
}
