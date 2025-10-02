using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Middleware
{
    public class RequiredFieldsMiddleware
    {
        private readonly RequestDelegate _next;

        public RequiredFieldsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Chỉ kiểm tra cho POST /api/books
            if (context.Request.Path.StartsWithSegments("/api/books")
                && context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                context.Request.EnableBuffering(); // cho phép đọc nhiều lần
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // reset lại stream để controller vẫn đọc được

                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(body);
                        var root = jsonDoc.RootElement;

                        var missingFields = new List<string>();

                        if (!root.TryGetProperty("title", out _)) missingFields.Add("title");
                        if (!root.TryGetProperty("authorIds", out _)) missingFields.Add("authorIds");
                        if (!root.TryGetProperty("publisherId", out _)) missingFields.Add("publisherId");

                        if (missingFields.Any())
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(JsonSerializer.Serialize(new
                            {
                                error = "MissingRequiredFields",
                                message = $"Missing fields: {string.Join(", ", missingFields)}"
                            }));
                            return;
                        }
                    }
                    catch (JsonException)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Invalid JSON format");
                        return;
                    }
                }
            }

            // Cho qua middleware tiếp theo
            await _next(context);
        }
    }
}
