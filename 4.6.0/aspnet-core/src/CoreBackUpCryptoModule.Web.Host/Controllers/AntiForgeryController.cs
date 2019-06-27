using Microsoft.AspNetCore.Antiforgery;
using CoreBackUpCryptoModule.Controllers;

namespace CoreBackUpCryptoModule.Web.Host.Controllers
{
    public class AntiForgeryController : CoreBackUpCryptoModuleControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public void GetToken()
        {
            _antiforgery.SetCookieTokenAndHeader(HttpContext);
        }
    }
}
