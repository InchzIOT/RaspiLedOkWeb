using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace RaspiLedOkWeb.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAuthorizationFilter
    {
        private const string API_KEY_PARAM = "apikey";
        private readonly string _configuredApiKey;

        public ApiKeyAuthAttribute()
        {
            // Hardcoded default fallback (optional)
            _configuredApiKey = "inCH3i0T";
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
            var expectedKey = config?["AppSettings:ApiKey"] ?? _configuredApiKey;

            // Support both query string ?apikey= and header x-api-key
            string providedKey =
                context.HttpContext.Request.Query[API_KEY_PARAM].FirstOrDefault() ??
                context.HttpContext.Request.Headers["x-api-key"].FirstOrDefault();

            if (string.IsNullOrEmpty(providedKey) || providedKey != expectedKey)
            {
                context.Result = new JsonResult(new { success = false, message = "Invalid or missing API key" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
        }
    }
}
