using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data.Context;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        // Represents the next middleware delegate in the HTTP request pipeline.
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Middleware for logging API requests.
        /// Captures request details like IP, request body, and username from the auth token (if present).
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="_logRepository">The repository for storing log entries.</param>
        public async Task Invoke(HttpContext httpContext, 
            ILogRepository _logRepository)
        {
            var injectedRequestStream = new MemoryStream();
            try
            {
                string requestBody = null;

                using (var bodyReader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    requestBody = await bodyReader.ReadToEndAsync();
                }
                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    var bytesToWrite = Encoding.UTF8.GetBytes(requestBody);
                    await injectedRequestStream.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);
                    injectedRequestStream.Seek(0, SeekOrigin.Begin);
                    httpContext.Request.Body = injectedRequestStream;
                }

                // Fetch username from auth token or keep it blank
                var username = ExtractUsernameFromToken(httpContext.Request.Headers["Authorization"]);

                // Store log entry in the database
                var logs = new Log
                {
                    Timestamp = DateTime.Now,
                    IpAddress = ConvertToIPv4Format(httpContext.Connection.RemoteIpAddress),
                    RequestBody = requestBody,
                    Username = username
                };
                await _logRepository.AddAsync(logs);
                await _logRepository.SaveChangesAsync();

                await _next(httpContext);
            }
            finally
            {
                // Dispose of the injected request stream to release resources.
                injectedRequestStream.Dispose();
            }
        }

        /// <summary>
        /// Converts an IP address to IPv4 format if it's an IPv6 address.
        /// </summary>
        /// <param name="ipAddress">The IP address to convert.</param>
        /// <returns>The IP address in IPv4 format or the original IPv4 address.</returns>
        private string ConvertToIPv4Format(IPAddress ipAddress)
        {
            if (ipAddress != null)
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    // Convert IPv6 address to IPv4 format
                    return IPAddress.Loopback.ToString();
                }
                else
                {
                    // Use the original IPv4 address
                    return ipAddress.ToString();
                }
            }
            return string.Empty;
        }


        /// <summary>
        /// Extracts the username from an authorization token.
        /// </summary>
        /// <param name="authorizationHeader">The authorization header containing the JWT token.</param>
        /// <returns>The extracted username or an empty string if not found or on error.</returns>
        private string ExtractUsernameFromToken(string authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return string.Empty; 
            }

            var jwtToken = authorizationHeader.Substring("Bearer ".Length);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(jwtToken);

                // Extract the username claim from the token
                var usernameClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name);

                if (usernameClaim != null)
                {
                    return usernameClaim.Value;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            return string.Empty; 
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}

