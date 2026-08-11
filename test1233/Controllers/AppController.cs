using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using test1233.Services;

namespace test1233.Controllers;

public abstract class AppController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        ViewBag.HasUsedTokens = _userStore.GetAllUsedTokens().Any();
        ViewBag.WelcomeMessage = "Welcome to our application!";
    }
}
