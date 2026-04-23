using DapperProject.Dtos.CustomerDtos;
using DapperProject.Services.CustomerServices;
using Microsoft.AspNetCore.Mvc;

namespace DapperProject.Controllers
{
    public class Customers : Controller
    {
        private readonly ICustomerService _customerService;

        public Customers(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public IActionResult Index()
        {
            var values = _customerService.GetAllCustomerAsync();
            return View(values);
        }
        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult>CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            await _customerService.CreateCustomerAsync(createCustomerDto);
            return RedirectToAction("CustomerList");
        }
        public async Task<IActionResult>DeleteCustomer(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return RedirectToAction("CustomerList");
        }
        [HttpGet]
        public async Task<IActionResult> UpdateCustomer(int id)
        {
            var value=await _customerService.GetCustomerByIdAsync(id);
            return View(value);
        }
        [HttpPost]
        public async Task<IActionResult>UpdateCustomer(UpdateCustomerDto updateCustomerDto)
        {
            await _customerService.UpdateCustomerAsync(updateCustomerDto);
            return RedirectToAction("CustomerList");
        }
    }
}
