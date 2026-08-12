using Microsoft.AspNetCore.Mvc;
using test1233.Services;

namespace test1233.Controllers;

public class RiaanController(ICalculationService calculationService) : Controller
{
    private readonly ICalculationService _calculationService = calculationService;

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Add(int left, int right)
    {
        var result = _calculationService.Add(left, right);

        return Json(new
        {
            left,
            right,
            result
        });
    }
}
