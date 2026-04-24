using Dapper;
using DapperProject.Context;
using DapperProject.Dtos.ProductDtos;

namespace DapperProject.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly DapperContext _context;

        public ProductService(DapperContext context)
        {
            _context = context;
        }

        public async Task CreateProductAsync(CreateProductDto createProductDto)
        {
            string query = "Insert Into Products (ProductName, ProductStock, ProductPrice, CategoryId) values (@productname, @productstock, @productprice, @categoryid)";

            var parameters = new DynamicParameters();
            parameters.Add("@productname", createProductDto.ProductName);
            parameters.Add("@productstock", createProductDto.ProductStock);
            parameters.Add("@productprice", createProductDto.ProductPrice);
            parameters.Add("@categoryid", createProductDto.CategoryId);

            var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, parameters);
        }

        public async Task DeleteProductAsync(int id)
        {
            string query = "Delete From Products Where ProductId=@productid";
            var parameters = new DynamicParameters();
            parameters.Add("@productid", id);
            var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, parameters);
        }

        public async Task<List<ResultProductDto>> GetAllProductAsync()
        {
            string query = "Select * From Products";
            var connection = _context.CreateConnection();
            var values = await connection.QueryAsync<ResultProductDto>(query);
            return values.ToList();
        }

        public async Task<GetProductByIdDto> GetProductByIdAsync(int id)
        {
            string query = "Select * From Products where ProductId=@id";
            var parameters = new DynamicParameters();
            parameters.Add("@id", id);
            var connection = _context.CreateConnection();
            var value = await connection.QueryFirstAsync<GetProductByIdDto>(query, parameters);
            return value;
        }

        public async Task UpdateProductAsync(UpdateProductDto updateProductDto)
        {
            string query = "Update Products Set ProductName=@productname, ProductStock=@productstock, ProductPrice=@productprice, CategoryId=@categoryid where ProductId=@productid";
            var parameters = new DynamicParameters();
            parameters.Add("@productname", updateProductDto.ProductName);
            parameters.Add("@productstock", updateProductDto.ProductStock);
            parameters.Add("@productprice", updateProductDto.ProductPrice);
            parameters.Add("@categoryid", updateProductDto.CategoryId);
            parameters.Add("@productid", updateProductDto.ProductId);
            var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, parameters);
        }
    }
}
