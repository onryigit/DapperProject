using DapperProject.Dtos.ProductDtos;

namespace DapperProject.Services.ProductServices
{
    public interface IProductService
    {
        Task<List<ResultProductDto>> GetAllProductAsync();
        Task<GetProductByIdDto> GetProductByIdAsync(int id);
        Task CreateProductAsync(CreateProductDto createProductDto);
        Task UpdateProductAsync(UpdateProductDto updateProductDto);
        Task DeleteProductAsync(int id);
    }
}