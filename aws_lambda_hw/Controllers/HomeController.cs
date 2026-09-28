using System.Diagnostics;
using aws_lambda_hw.Models;
using aws_lambda_hw.Services;
using Microsoft.AspNetCore.Mvc;

namespace aws_lambda_hw.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SqsService _sqsService;

    public HomeController(
        ILogger<HomeController> logger,
        SqsService sqsService)
    {
        _logger = logger;
        _sqsService = sqsService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new OrderRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        OrderRequest order,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(order);

        try
        {
            await _sqsService.SendOrderAsync(order, cancellationToken);

            TempData["Success"] = "Your order has been accepted for processing.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to submit order for {order.CustomerName}");

            ModelState.AddModelError(string.Empty, "Unable to submit your order. Please try again.");

            return View(order);
        }
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}