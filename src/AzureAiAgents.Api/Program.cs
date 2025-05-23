using AzureAiAgents.Api.CodeInterpreterTool;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
//builder.Services.Configure()

builder.Services.AddScoped<CodeInterpreterToolService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.RegisterCodeInterpreterTool();