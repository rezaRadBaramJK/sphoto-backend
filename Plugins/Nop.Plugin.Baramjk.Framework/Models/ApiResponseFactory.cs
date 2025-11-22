using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Nop.Plugin.Baramjk.Framework.Extensions;

namespace Nop.Plugin.Baramjk.Framework.Models
{
    public static class ApiResponseFactory
    {
        public static ApiResponse Success(string message = "Request process successfully.")
        {
            return new ApiResponse
            {
                Data = null,
                StatusCode = StatusCodes.Status200OK,
                Message = message,
                IsSuccess = true
            };
        }

        public static ApiResponse Create(string message = "Request process successfully.")
        {
            return new ApiResponse
            {
                Data = null,
                StatusCode = StatusCodes.Status201Created,
                Message = message,
                IsSuccess = true
            };
        }

        public static ApiResponse BadRequest(string messages = "BadRequest")
        {
            var response = new ApiResponse
            {
                Data = null,
                StatusCode = StatusCodes.Status400BadRequest,
                IsSuccess = false,
                Message = messages
            };

            return response;
        }

        public static ApiResponse BadRequest(string messages, params string[] errors)
        {
            var response = new ApiResponse
            {
                Data = null,
                StatusCode = StatusCodes.Status400BadRequest,
                IsSuccess = false,
                Message = messages
            };

            response.Errors.AddRange(errors);

            return response;
        }

        public static ApiResponse BadRequest(string messages, IEnumerable<string> errors)
        {
            var response = new ApiResponse
            {
                Data = null,
                StatusCode = StatusCodes.Status400BadRequest,
                IsSuccess = false,
                Message = messages ?? errors.FirstOrDefault("BadRequest")
            };

            response.Errors.AddRange(errors);

            return response;
        }

        public static ApiResponse NotFound(string message = "NotFound")
        {
            return new ApiResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = message,
                IsSuccess = false
            };
        }

        public static ApiResponse Unauthorized(string message = "Unauthorized access.")
        {
            return new ApiResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = message,
                IsSuccess = false
            };
        }

        public static ApiResponse InternalServerError(string messages)
        {
            var response = new ApiResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                IsSuccess = false,
                Message = messages
            };

            return response;
        }

        public static ApiResponse<T> Auto<T>(bool status, T data)
        {
            return status ? Success(data) : BadRequest(data);
        }

        public static ApiResponse<T> Success<T>(T data, string message = "Request process successfully.")
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = message,
                Data = data,
                IsSuccess = true
            };
        }

        public static ApiResponse<T> Create<T>(T data, string message = "Request process successfully.")
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = message,
                Data = data,
                IsSuccess = true
            };
        }

        public static ApiResponse<T> NotFound<T>(T data, string message = "Not Found")
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = message,
                Data = data,
                IsSuccess = false
            };
        }

        public static ApiResponse<T> Unauthorized<T>(T data, string message = "Unauthorized access.")
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = message,
                Data = data,
                IsSuccess = false
            };
        }

        public static ApiResponse<T> BadRequest<T>(T data, string messages)
        {
            var response = new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                IsSuccess = false,
                Message = messages,
                Data = data
            };

            return response;
        }

        public static ApiResponse<T> BadRequest<T>(T data)
        {
            var response = new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                IsSuccess = false,
                Message = "BadRequest",
                Data = data
            };

            return response;
        }

        public static ApiResponse<T> InternalServerError<T>(T data, string messages)
        {
            var response = new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                IsSuccess = false,
                Message = messages,
                Data = data
            };

            return response;
        }
    }
}