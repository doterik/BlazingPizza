using BlazingPizza.ComponentsLibrary.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace BlazingPizza.Server
{
    [ApiController]
    public class UserController : Controller
    {
        private static UserState LoggedOutState = new UserState { IsLoggedIn = false };

        [HttpGet("user")]
        public UserState GetUser()
        {
            return User.Identity.IsAuthenticated
                ? new UserState { IsLoggedIn = true, DisplayName = User.Identity.Name }
                : LoggedOutState;
        }

        [HttpGet("user/signin")]
        public async Task SignIn()
        {
            await HttpContext.ChallengeAsync(
                MicrosoftAccountDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = "/user/signincompleted" });
        }

        [Authorize]
        [HttpGet("user/signincompleted")]
        public IActionResult SignInCompleted()
        {
            var userState = GetUser();
            var json = JsonConvert.SerializeObject(
                userState, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return Content($@"
                <script>
                    window.opener.onLoginPopupFinished({json});
                    window.close();
                </script>", "text/html");
        }

        [HttpPut("user/signout")]
        public async Task<UserState> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LoggedOutState;
        }
    }
}
