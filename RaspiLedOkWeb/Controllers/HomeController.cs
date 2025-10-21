using Doggo.Data.Models;
using Microsoft.AspNetCore.Mvc;
using RaspiLedOkWeb.Filters;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using RaspiLedOkWeb.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaspiLedOkWeb.Controllers
{
    public class HomeController : Controller
    {
        private enum DeviceType
        {
            Air,
            Ph
        }

        private enum AirSensorsKey
        {
            So2,
            No2,
            Temperature,
            Humidity,
            Pm25,
            Pm10
        }

        private readonly ILogger<HomeController> _logger;
        private readonly ISyncService _syncService;
        private readonly IApiConfigurationService _apiConfigurationService;
        private static AirSensorModel cacheAirSensorModel;
        private static PoleSensorModel cacheAirAndWaterSensorModel = new PoleSensorModel();
        private static bool isLogin = true;

        //private static Dictionary<DeviceType<Dictionary<AirSensorsKey, string>>() keyValuesPairs;
        public HomeController(ILogger<HomeController> logger, ISyncService syncService, IApiConfigurationService apiConfigurationService)
        {
            _logger = logger;
            _syncService = syncService;
            _apiConfigurationService = apiConfigurationService;
        }

        [ApiKeyAuth]
        public async Task<IActionResult> Index()
        {
            try
            {

                var loginRes = await _syncService.AutoLogin();
                if (!loginRes.Success)
                {
                    isLogin = false;
                }

                var configRes = _apiConfigurationService.GetConfiguration();
                if (!_apiConfigurationService.ValidateConfiguration(configRes))
                {
                    return View("ErrorPage");
                }
                
                ViewBag.Height = (configRes.Screen.Height <= 0 ? 400 : configRes.Screen.Height) + "px";
                ViewBag.Width = (configRes.Screen.Width <= 0 ? 200 : configRes.Screen.Width) + "px";
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home/Index");
                return View("ErrorPage");
            }
            
        }

        [ApiKeyAuth]
        public async Task<IActionResult> IndexDemo()
        {
            try
            {
                var configRes = _apiConfigurationService.GetConfiguration();
                if (!_apiConfigurationService.ValidateConfiguration(configRes))
                {
                    return View("ErrorPage");
                }

                var loginRes = await _syncService.AutoLogin();
                if (!loginRes.Success)
                {
                    return View("ErrorPage");
                }

                ViewBag.Height = (configRes.Screen.Height <= 0 ? 240 : configRes.Screen.Height) + "px";
                ViewBag.Width = (configRes.Screen.Width <= 0 ? 120 : configRes.Screen.Width) + "px";
                ViewBag.Width = "120px";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home/Index");
                return View("ErrorPage");
            }

        }

        public async Task<IActionResult> Logo()
        {
            try
            {
                var configRes = _apiConfigurationService.GetConfiguration();
                if (!_apiConfigurationService.ValidateConfiguration(configRes))
                {
                    return View("ErrorPage");
                }

                var loginRes = await _syncService.AutoLogin();
                if (!loginRes.Success)
                {
                    return View("ErrorPage");
                }
                ViewBag.Height = configRes.Screen.Height + "px";
                ViewBag.Height = configRes.Screen.Width + "px";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home/Index");
                return View("ErrorPage");
            }
            
        }

        public async Task<IActionResult> Combine()
        {
            try
            {
                var configRes = _apiConfigurationService.GetConfiguration();
                if (!_apiConfigurationService.ValidateConfiguration(configRes))
                {
                    return View("ErrorPage");
                }

                var loginRes = await _syncService.AutoLogin();
                if (!loginRes.Success)
                {
                    return View("ErrorPage");
                }
                ViewBag.Height = configRes.Screen.Height + "px";
                ViewBag.Height = configRes.Screen.Width + "px";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home/Index");
                return View("ErrorPage");
            }
        }

        public async Task<IActionResult> Test()
        {
            try
            {
                var configRes = _apiConfigurationService.GetConfiguration();
                if (!_apiConfigurationService.ValidateConfiguration(configRes))
                {
                    return View("ErrorPage");
                }

                var loginRes = await _syncService.AutoLogin();
                if (!loginRes.Success)
                {
                    return View("ErrorPage");
                }
                ViewBag.Height = configRes.Screen.Height + "px";
                ViewBag.Height = configRes.Screen.Width + "px";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home/Index");
                return View("ErrorPage");
            }
        }

        [ApiKeyAuth]
        public async Task<IActionResult> IndexV2()
        {
            try
            {
                var configRes = _apiConfigurationService.GetConfiguration();
                if (!_apiConfigurationService.ValidateConfiguration(configRes))
                {
                    return View("ErrorPage");
                }

                var loginRes = await _syncService.AutoLogin();
                if (!loginRes.Success)
                {
                    return View("ErrorPage");
                }
                ViewBag.Height = configRes.Screen.Height + "px";
                ViewBag.Height = configRes.Screen.Width + "px";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home/Index");
                return View("ErrorPage");
            }
        }

        public IActionResult ErrorPage()
        {
            return View();
        }

        public async Task<JsonResult> GetAirSensorData()
        {
            AirSensorModel res = await _syncService.GetAirSensorLatestDataByDeviceIdAsync(1);
            return Json(new { res });
        }

        public List<Asset> GetAssets()
        {
            // Get the configured assets from appsettings.json
            var assets = _apiConfigurationService.GetAssets();
            var enabledAssets = assets.Where(a => a.IsEnabled).ToList();
            return enabledAssets;
        }

       public List<Device> GetDevicesByAsset(string assetId)
       {
            var assets = _apiConfigurationService.GetAssets();
            List<Device> devices = assets.Where(x => x.IsEnabled && x.Id == assetId).Select(x=>x.Devices).FirstOrDefault().Where(x => x.IsEnabled).ToList();
            return devices;
       }

        public async Task<JsonResult> GetAirSensorValue()
        {
            try
            {
                await _syncService.AutoLogin();
                bool success = true;
                var asset = GetAssets().Where(x=>x.IsEnabled).FirstOrDefault();
                var device = GetDevicesByAsset(asset.Id.ToString()).FirstOrDefault(x => x.Name.ToLower().Contains(DeviceType.Air.ToString().ToLower()));
                if (device != null)
                {
                    var res = await _syncService.GetAirSensorLatestDataByDeviceIdAsync(int.Parse(device.Id));
                    if (res.Co != null && res.No2 != null && res.So2 != null && res.Humidity != null && res.Temperature != null)
                    {
                        cacheAirSensorModel = res;
                    }
                    return Json(new { success, data = cacheAirSensorModel });
                }
                else
                {
                    return Json(new { success = true, message = "No air device had found", data = cacheAirSensorModel });
                }
            }  catch(Exception ex)
            {
                return Json(new { success = true, message = "No air device had found", data = cacheAirSensorModel });
            }

        }

        public async Task<JsonResult> GetAirAndWaterSensorValue()
        {
            try
            {
                bool success = true;
                if(isLogin == false)
                {
                    var loginRes = await _syncService.AutoLogin();
                    if (loginRes.Success)
                    {
                        isLogin = true;
                    }
                    else
                    {
                        isLogin = false;
                    }
                }

                var asset = GetAssets().Where(x=>x.IsEnabled).FirstOrDefault();
                var airDevice = GetDevicesByAsset(asset.Id.ToString()).FirstOrDefault(x => x.Name.ToLower().Contains(DeviceType.Air.ToString().ToLower()));
                var waterDevice = GetDevicesByAsset(asset.Id.ToString()).FirstOrDefault(x => x.Name.ToLower().Contains(DeviceType.Ph.ToString().ToLower()));
                if (airDevice != null)
                {
                    var res = await _syncService.GetAirAndWaterSensorLatestDataByDeviceIdAsync(int.Parse(airDevice.Id), int.Parse(waterDevice.Id));
                    if (res.pH != null && res.Co != null && res.No2 != null && res.So2 != null && res.Humidity != null && res.Temperature != null)
                    {
                        cacheAirAndWaterSensorModel = res;
                    }
                    return Json(new { success, data = res });
                }
                else
                {
                    return Json(new { success = true, message = "No air device had found", data = cacheAirAndWaterSensorModel });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = true, message = "No air and water device had found", data = cacheAirAndWaterSensorModel });
            }

        }
    }
}
