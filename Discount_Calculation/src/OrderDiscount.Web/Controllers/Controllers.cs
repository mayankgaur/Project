using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrderDiscount.Application.Interfaces;
using OrderDiscount.Domain.Entities;
using OrderDiscount.Domain.Exceptions;
using OrderDiscount.Web.Models;
using OrderDiscount.Web.Resources;

namespace OrderDiscount.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/discounts")]
public sealed class DiscountsApiController(IOrderDiscountService service, IDiscountStrategyResolver resolver) : ControllerBase
{
    [HttpPost("calculate")]
    public async Task<ActionResult<CalculateDiscountResponse>> Calculate([FromBody] CalculateDiscountRequest req, CancellationToken ct)
    {
        var result = await service.CalculateAsync(new Order(req.OrderId, req.CustomerName, req.OrderAmount, req.CustomerType), ct);
        return Ok(CalculateDiscountResponse.FromDomain(result));
    }

    [HttpGet("customer-types")]
    public ActionResult<IReadOnlyCollection<string>> GetCustomerTypes() => Ok(resolver.GetSupportedCustomerTypes());
}

[AllowAnonymous]
[Route("")]
[Route("orders")]
public sealed class OrdersController(IOrderDiscountService service, IDiscountStrategyResolver resolver, IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet]
    public IActionResult Index([FromQuery] string? culture) => View(new OrderFormViewModel
    {
        Culture = string.IsNullOrWhiteSpace(culture) ? CultureInfo.CurrentUICulture.Name : culture,
        AvailableCustomerTypes = resolver.GetSupportedCustomerTypes()
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(OrderFormViewModel model, CancellationToken ct)
    {
        model.AvailableCustomerTypes = resolver.GetSupportedCustomerTypes();
        try
        {
            var result = await service.CalculateAsync(new Order(model.OrderId, model.CustomerName, model.OrderAmount ?? 0m, model.CustomerType), ct);
            model.Result = CalculateDiscountResponse.FromDomain(result);
        }
        catch (DomainValidationException ex)
        {
            var loc = localizer[ex.ErrorCode, ex.Args];
            model.ErrorMessage = loc.ResourceNotFound ? ex.Message : loc.Value;
        }
        return View(model);
    }
}