using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using CoreBackUpCryptoModule.Controllers;

namespace CoreBackUpCryptoModule.Web.Controllers
{
    [AbpMvcAuthorize]
    public class HomeController : CoreBackUpCryptoModuleControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
	}
}
