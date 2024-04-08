using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using BankSimService.Models.Contracts;
using BankSimService.Models;

namespace BankSimService.Actions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string ApiKeyHeaderName = "Key";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                //Before
                //First try to find the apiKey header in the request header
                if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
                {
                    //should the Http status code be 401?
                    context.Result = new OkObjectResult(new CreateAccountResponse
                    {
                       ResponseHeader = new ResponseHeader
                       {
                           ResponseCode = "01",
                           ResponseMessage = "API KEY IS REQUIRED"
                       }
                    });
                    return;
                }

                var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var apiKey = configuration.GetValue<string>("ApiKey");

                if (!apiKey.Equals(potentialApiKey))
                {
                    context.Result = new OkObjectResult(new CreateAccountResponse
                    {
                       ResponseHeader = new ResponseHeader
                       {
                           ResponseCode = "01",
                           ResponseMessage = "INVALID API KEY"
                       }
                    });
                    return;
                }

                await next();

                //after
            }
            catch (Exception ex)
            {
                Utilities.WriteLog("CardPaymentController", $"ERR: {ex.Message}");
                context.Result = new OkObjectResult(new CreateAccountResponse
                {
                   ResponseHeader = new ResponseHeader
                   {
                       ResponseCode = "01",
                       ResponseMessage = "System Challenge"
                   }
                });

                return;
            }
        }
    }
}
