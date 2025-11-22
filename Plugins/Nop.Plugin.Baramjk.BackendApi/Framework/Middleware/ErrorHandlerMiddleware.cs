using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Nop.Plugin.Baramjk.BackendApi.Framework.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private const string DeleteConstraint = "The DELETE statement conflicted with the REFERENCE constraint";
        private const string ForeignKeyConstraint = "conflicted with the FOREIGN KEY constraint";

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new() { NullValueHandling = NullValueHandling.Ignore };

        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly ISettingService _settingService;

        public ErrorHandlerMiddleware(ILogger logger, ISettingService settingService, RequestDelegate next)
        {
            _next = next;
            _logger = logger;
            _settingService = settingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var response = exception switch
                {
                    BusinessException businessException => BusinessExceptionHandler(businessException),
                    SqlException sqlException => SqlExceptionExceptionHandler(sqlException),
                    NullReferenceException => ApiResponseFactory.InternalServerError(exception, "NullReference"),
                    _ => ApiResponseFactory.InternalServerError(exception, "Error")
                };

                //_logger.LogError(exception, "ErrorHandlerMiddleware");
                await WriteAsync(context, response);
            }
        }

        private static ApiResponse BusinessExceptionHandler(BusinessException businessException)
        {
            switch (businessException)
            {
                case NotFoundBusinessException:
                    return ApiResponseFactory.NotFound(businessException, businessException.Message);
                case UnauthorizedBusinessException:
                    return ApiResponseFactory.Unauthorized(businessException, businessException.Message);
                case DuplicationBusinessException:
                case BadRequestBusinessException:
                    return ApiResponseFactory.BadRequest(businessException, businessException.Message);
                case InternalErrorBusinessException:
                    return ApiResponseFactory.InternalServerError(businessException, businessException.Message);
                default:
                    return ApiResponseFactory.BadRequest(businessException, businessException.Message);
            }
        }

        private static ApiResponse SqlExceptionExceptionHandler(SqlException exception)
        {
            string message;
            switch (exception.Number)
            {
                case 515:
                    message = "Db Error. Fill require filed";
                    break;
                case 2628:
                    message = "Db Error. Data is big";
                    break;
                case 2627:
                case 2601:
                    message = "Db Error. Duplicate records";
                    break;
                case 547: //constraint
                    if (exception.Message.Contains(DeleteConstraint))
                        message = "Db Error. DELETE statement conflicted with the reference";
                    else if (exception.Message.Contains(ForeignKeyConstraint))
                        message = "Db Error. Conflicted with the foreign key constraint";
                    else
                        message = "Db Error. Constraint Error";
                    break;
                default:
                    message = "Db Error";
                    break;
            }

            return ApiResponseFactory.InternalServerError(exception, message);
        }

        private static Task WriteAsync(HttpContext context, ApiResponse response)
        {
            context.Response.StatusCode = response.StatusCode;
            context.Response.ContentType = "application/json; charset=UTF-8";
            var jsonContent = JsonConvert.SerializeObject(response, JsonSerializerSettings);
            return context.Response.WriteAsync(jsonContent, Encoding.UTF8);
        }

        private async Task NewMethod(HttpContext context, Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            response.StatusCode = error switch
            {
                NopException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var apiSettings = await _settingService.LoadSettingAsync<WebApiBackendSettings>();

            var data = new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                IsSuccess = false,
                error.Message,
                InnerExceptionMessage = error.InnerException?.Message,
                StackTrace = apiSettings.DeveloperMode ? error.ToString() : string.Empty
            };

            var result = JsonSerializer.Serialize(data);

            await _logger.ErrorAsync(error.Message, error);

            await response.WriteAsync(result);
        }
    }
}