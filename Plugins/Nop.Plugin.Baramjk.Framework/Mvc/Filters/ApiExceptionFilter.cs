using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Mvc.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var response = context.Exception switch
            {
                BusinessException businessException => BusinessExceptionHandler(businessException),
                NullReferenceException => ApiResponseFactory.InternalServerError(context.Exception, "NullReference"),
                _ => ApiResponseFactory.InternalServerError(context.Exception, "Error")
            };

            context.Result = response;
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
    }
}