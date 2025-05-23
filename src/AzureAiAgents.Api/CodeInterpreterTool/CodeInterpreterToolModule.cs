using Microsoft.AspNetCore.Mvc;

namespace AzureAiAgents.Api.CodeInterpreterTool;

public static class CodeInterpreterToolModule
{
    public static void RegisterCodeInterpreterTool(this IEndpointRouteBuilder app)
    {
        app.MapGet("/code-interpreter/create-agent", async (
                [FromServices] CodeInterpreterToolService service,
                [FromForm] FormFile file) =>
        {
            var agentId = await service.CreateAgentWithCodeInterpreterTool(file.OpenReadStream(), file.FileName);
            return Results.Ok(agentId);
        })
        .WithTags("CodeInterpreterTool");
        
        app.MapGet("/code-interpreter/create-thread", async (
                [FromServices] CodeInterpreterToolService service,
                [FromForm] FormFile file) =>
            {
                var agentId = await service.CreateThreadAsync();
                return Results.Ok(agentId);
            })
            .WithTags("CodeInterpreterTool");
        
        app.MapGet("/code-interpreter/run", async (
                [FromServices] CodeInterpreterToolService service,
                [FromQuery] string agentId,
                [FromQuery] string threadId,
                [FromQuery] string userInput) =>
            {
                var result = await service.RunCodeInterpreterTool(agentId, threadId, userInput);
                return Results.Ok(result);
            })
            .WithTags("CodeInterpreterTool");
    }
}