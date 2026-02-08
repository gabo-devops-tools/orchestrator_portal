using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace orchestrator_portal.Controllers
{
    public class AccountController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = Url.Action("CreateProjects", "Home")
            },
            OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            return SignOut(
                  new AuthenticationProperties
                  {
                      RedirectUri = "/Account/Index"
                  },
                  OpenIdConnectDefaults.AuthenticationScheme
              );
        }
    }
}
