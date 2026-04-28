using CRM.Core.Interfaces;
using CRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.Controllers;

[Authorize]
public class CustomerController : Controller
{
    private readonly ICustomerRepository _customers;
    public CustomerController(ICustomerRepository customers) => _customers = customers;

    public async Task<IActionResult> Index()
    {
        var all = (await _customers.GetAllAsync()).ToList();
        var vm = new CustomerListViewModel
        {
            TotalRevenue = all.Sum(c => c.DealValue),
            TotalCount = all.Count,
            Customers = all.Select(c => new CustomerRowItem
            {
                Id = c.Id, FullName = c.FullName, Phone = c.Phone,
                PropertyPurchased = c.PropertyPurchased, DealValue = c.DealValue,
                DealClosedDate = c.DealClosedDate, AgentName = c.Agent?.FullName ?? "", Source = c.Source
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await _customers.GetByIdAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }
}
