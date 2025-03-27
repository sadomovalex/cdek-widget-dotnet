using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Text;
using Newtonsoft.Json;

namespace CdekExample;

internal class CdekMiddleware
{    
    public CdekMiddleware(RequestDelegate next)
    {
    }

    public async Task Invoke(HttpContext context, ICdekService cdekService)
    {
        var requestParams = await this.getRequestParams(context);

        object action = null;
        requestParams?.TryGetValue("action", out action);
        if (string.IsNullOrEmpty(action as string))
        {
            await this.sendValidationError(context, "Action is required");
            return;
        }

        if (string.Compare(action as string, "offices", true) == 0)
        {
            var result = cdekService.GetOffices(requestParams);
            await this.writeResponseAsync(context, result);
            return;
        }
        if (string.Compare(action as string, "calculate", true) == 0)
        {
            var calculations = cdekService.Calculate(requestParams);
            await this.writeResponseAsync(context, calculations);
            return;
        }

        await this.sendValidationError(context, "Unknown action");
    }

    private async Task<Dictionary<string, object>> getRequestParams(HttpContext ctx)
    {
        var data = new Dictionary<string, object>();

        foreach (var kv in ctx.Request.Query)
        {
            data[kv.Key] = kv.Value.ToString();
        }

        var stream = ctx.Request.Body;
        string json = await new StreamReader(stream).ReadToEndAsync();
        if (!string.IsNullOrEmpty(json))
        {
            var body = JsonConvert.DeserializeObject<ExpandoObject>(json) as IDictionary<String, Object>;
            body?.ToList().ForEach(kv => { data[kv.Key] = kv.Value; });
        }

        return data;
    }

    private async Task sendValidationError(HttpContext context, string msg)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await this.writeResponseAsync(context, new CdekResponse{Data = msg});
    }

    private async Task writeResponseAsync(HttpContext context, CdekResponse resp)
    {
        resp?.Headers?.ToList().ForEach(kv =>
        {
            context.Response.Headers.TryAdd(kv.name, kv.value);
        });
        await context.Response.WriteAsJsonAsync((object)resp?.Data);
    }
}

internal static class CdeknMiddlewareExtension
{
    public static IApplicationBuilder UseCdekMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CdekMiddleware>();
    }
}