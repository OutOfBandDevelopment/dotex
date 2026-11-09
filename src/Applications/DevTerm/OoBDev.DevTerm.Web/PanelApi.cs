using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace OoBDev.DevTerm.Web;

internal static class PanelApi
{
    public static async Task<IResult> InvokeAsync(HttpContext context, OoBDev.UiDefinitions.IControlSurface surface, InvokeRequest request)
    {
        if (context.Items.ContainsKey(AccessTokenMiddleware.ReadOnlyItem))
        {
            return Results.Json(new { error = "read-only viewer: controls are not allowed." }, statusCode: StatusCodes.Status403Forbidden);
        }

        if (string.IsNullOrWhiteSpace(request.CommandId))
        {
            return Results.BadRequest(new { error = "commandId is required." });
        }

        try
        {
            await surface.InvokeAsync(request.CommandId, request.Value, context.RequestAborted).ConfigureAwait(false);
            return Results.Ok(new { ok = true });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
