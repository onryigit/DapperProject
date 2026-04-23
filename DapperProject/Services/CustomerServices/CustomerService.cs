using Dapper;
using DapperProject.Context;
using DapperProject.Dtos.CustomerDtos;

namespace DapperProject.Services.CustomerServices
{
    public class CustomerService : ICustomerService
    {
        private readonly DapperContext _context;

        public CustomerService(DapperContext context)
        {
            _context = context;
        }

        public async Task CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            string query = "insert into Customers (CustomerName, CustomerSurname, CustomerCity) values (@customername,@customersurname,@customercity)";

            var parameters = new DynamicParameters();
            parameters.Add("@customername", createCustomerDto.CustomerName);
            parameters.Add("@customersurname", createCustomerDto.CustomerSurname);
            parameters.Add("@customercity", createCustomerDto.CustomerCity);

            var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, parameters);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            string query = "Delete From Customers Where CustomerId=@customerid";
            var parameters=new DynamicParameters();
            parameters.Add("customerid", id);
            var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, parameters);
        }

        public async Task<List<ResultCustomerDto>> GetAllCustomerAsync()
        {
            string query = "Select * From Customers";
            var connection = _context.CreateConnection();
            var values = await connection.QueryAsync<ResultCustomerDto>(query);
            return values.ToList();
        }

        public async Task<GetCustomerByIdDto> GetCustomerByIdAsync(int id)
        {
            string query = "Select * From Customers Where CustomerId=@id";
            var parameters = new DynamicParameters();
            parameters.Add("@id", id);
            var connection = _context.CreateConnection();
            var values = await connection.QueryFirstAsync<GetCustomerByIdDto>(query);
            return values;
        }
        public async Task UpdateCustomerAsync(UpdateCustomerDto updateCustomerDto)
        {
            string query = "Update Customers Set CustomerName=@customername,CustomerSurname=@customersurname,CustomerCity=@customercity where CustomerId=@customerId";

            var parameters = new DynamicParameters();
            parameters.Add("@customername", updateCustomerDto.CustomerName);
            parameters.Add("@customersurname", updateCustomerDto.CustomerSurname);
            parameters.Add("@customerCity", updateCustomerDto.CustomerCity);
            parameters.Add("@customerId", updateCustomerDto.CustomerId);

            var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, parameters);
        }
    }
}
