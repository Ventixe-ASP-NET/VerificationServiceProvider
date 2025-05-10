namespace WebApi.Middlewares;

public class DefaultApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;
    private const string APIKEY_HEADER_NAME = "X-API-KEY";

    public async Task InvokeAsync(HttpContext context)
    {
        var defaultApiKey = _configuration["SecretKeys:Default"] ?? null;

        if (string.IsNullOrEmpty(defaultApiKey) || !context.Request.Headers.TryGetValue(APIKEY_HEADER_NAME, out var providedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid api-key or api-key is missing.");
            return;
        }

        if (!string.Equals(providedApiKey, defaultApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid api-key.");
            return;
        }

        await _next(context);
    }
}