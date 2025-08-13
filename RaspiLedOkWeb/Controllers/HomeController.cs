using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using RaspiLedOkWeb.Services;

namespace RaspiLedOkWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISyncService _syncService;
        private readonly IApiConfigurationService _apiConfigurationService;

        public HomeController(ILogger<HomeController> logger, ISyncService syncService, IApiConfigurationService apiConfigurationService)
        {
            _logger = logger;
            _syncService = syncService;
            _apiConfigurationService = apiConfigurationService;
        }

        public async Task<IActionResult> Index()
        {
            var configRes = _apiConfigurationService.GetConfiguration();
            var loginRes = await _syncService.Login(configRes.Username, CryptoHelper.PreDecrypt(configRes.Password));
            _apiConfigurationService.
            return View();
        }
    }
}
