using System.Diagnostics;
using System.Threading.Tasks;
using Doggo.Data.Models;
using Microsoft.AspNetCore.Mvc;
using RaspiLedOkWeb.Helpers;
using RaspiLedOkWeb.Models;
using RaspiLedOkWeb.Services;

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
        private static PoleSensorModel cacheAirAndWaterSensorModel = new PoleSensorModel()
        {
            Temperature = "28.5",
            MinTemp = "24.0",
            MaxTemp = "32.0",
            Humidity = "70",
            Pm25 = "10",
            Pm10 = "10",
            O3 = "0.05",
            Co = "0.01",
            No2 = "0.03",
            So2 = "0.02",
            OverallAPI = "50",
            pH = "6.7",
            Message = "Using mock sensor data",
            Error = null,
            Success = true
        };

        //private static Dictionary<DeviceType<Dictionary<AirSensorsKey, string>>() keyValuesPairs;
        public HomeController(ILogger<HomeController> logger, ISyncService syncService, IApiConfigurationService apiConfigurationService)
        {
            _logger = logger;
            _syncService = syncService;
            _apiConfigurationService = apiConfigurationService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
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
                bool success = true;
                var asset = GetAssets().FirstOrDefault();
                var device = GetDevicesByAsset(asset.Id.ToString()).FirstOrDefault(x => x.Name.ToLower().Contains(DeviceType.Air.ToString().ToLower()));
                if (device != null)
                {
                    var res = await _syncService.GetAirSensorLatestDataByDeviceIdAsync(int.Parse(device.Id));
                    cacheAirSensorModel = res;
                    return Json(new { success, data = res });
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

                var loginRes = await _syncService.AutoLogin();
                if (!loginRes.Success)
                {
                    return Json(new { success = true, message = "No air device had found", data = cacheAirAndWaterSensorModel });
                }

                var asset = GetAssets().FirstOrDefault();
                var airDevice = GetDevicesByAsset(asset.Id.ToString()).FirstOrDefault(x => x.Name.ToLower().Contains(DeviceType.Air.ToString().ToLower()));
                var waterDevice = GetDevicesByAsset(asset.Id.ToString()).FirstOrDefault(x => x.Name.ToLower().Contains(DeviceType.Ph.ToString().ToLower()));
                if (airDevice != null && waterDevice != null)
                {
                    var res = await _syncService.GetAirAndWaterSensorLatestDataByDeviceIdAsync(int.Parse(airDevice.Id), int.Parse(waterDevice.Id));
                    cacheAirAndWaterSensorModel = res;
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
