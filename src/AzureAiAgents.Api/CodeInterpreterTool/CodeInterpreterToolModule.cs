using Microsoft.AspNetCore.Mvc;

namespace AzureAiAgents.Api.CodeInterpreterTool;

public static class CodeInterpreterToolModule
{
    public static void RegisterCodeInterpreterTool(this IEndpointRouteBuilder app)
    {
        //"basic-code-interpreter-agent"
        // "You are a helpful agent that can help fetch data from files you know about.",
        app.MapPost("/code-interpreter/create-agent/with-file", async (
                [FromServices] CodeInterpreterToolService service,
                [FromForm] IFormFile file,
                [FromQuery] string name,
                [FromQuery] string instructions) =>
        {
            var agentId = await service.CreateAgentAsync(file.OpenReadStream(), file.FileName, name, instructions);
            return Results.Ok(agentId);
        })
        .WithTags("CodeInterpreterTool")
        .DisableAntiforgery();
        
        //You are a helpful agent that can help fetch data from files you know about. Don't predict any data.
        app.MapPost("/code-interpreter/create-agent", async (
                [FromServices] CodeInterpreterToolService service,
                [FromQuery] string name, [FromQuery] string instructions) =>
            {
                var agentId = await service.CreateAgentAsync(name, instructions);
                return Results.Ok(agentId);
            })
            .WithTags("CodeInterpreterTool")
            .DisableAntiforgery();
        
        app.MapGet("/code-interpreter/create-thread", async (
                [FromServices] CodeInterpreterToolService service) =>
            {
                var agentId = await service.CreateThreadAsync();
                return Results.Ok(agentId);
            })
            .WithTags("CodeInterpreterTool");
        
        app.MapPost("/code-interpreter/create-thread/with-file", async (
                [FromServices] CodeInterpreterToolService service,
                [FromForm] IFormFile file) =>
            {
                var result = await service.CreateThreadAsync(file.OpenReadStream(), file.Name);
                return Results.Ok(result);
            })
            .WithTags("CodeInterpreterTool")
            .DisableAntiforgery();
        
        app.MapGet("/code-interpreter/run", async (
                [FromServices] CodeInterpreterToolService service,
                [FromQuery] string agentId,
                [FromQuery] string threadId,
                [FromQuery] string userInput) =>
            {
                var result = await service.RunAsync(agentId, threadId, userInput);
                return Results.Ok(result);
            })
            .WithTags("CodeInterpreterTool");
        
        app.MapGet("/code-interpreter/run/chart", async (
                [FromServices] CodeInterpreterToolService service,
                [FromQuery] string agentId,
                [FromQuery] string threadId,
                [FromQuery] string userInput) =>
            {
                var result = await service.RunWithAttachementsAsync(agentId, threadId, userInput);
                return Results.File(result, contentType: "image/png", "chart.png");
            })
            .WithTags("CodeInterpreterTool");
       
    }
}