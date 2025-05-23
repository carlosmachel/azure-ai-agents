using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace AzureAiAgents.Api.CodeInterpreterTool;

public class CodeInterpreterToolService(IOptions<AzureAiAgentSettings> options)
{
    public async Task<string> CreateAgentWithCodeInterpreterTool(Stream fileStream, string fileName)
    {
        var projectClient = new AIProjectClient(options.Value.ConnectionString, new DefaultAzureCredential());
        var agentClient = projectClient.GetAgentsClient();
        List<ToolDefinition> tools = [ new CodeInterpreterToolDefinition() ];

        AgentFile uploadAgentFile = await agentClient.UploadFileAsync(fileStream, purpose: AgentFilePurpose.Agents, fileName);
        
        Agent agent = await agentClient.CreateAgentAsync(options.Value.Model,
            name: "basic-code-interpreter-agent",
            instructions: "You are a helpful agent that can help fetch data from files you know about.",
            tools: tools,
            toolResources: new ToolResources()
            {
                CodeInterpreter = new CodeInterpreterToolResource
                {
                    FileIds = { uploadAgentFile.Id }
                }
            });
        return agent.Id;
    }

    public async Task<string> CreateThreadAsync()
    {
        var projectClient = new AIProjectClient(options.Value.ConnectionString, new DefaultAzureCredential());
        var agentClient = projectClient.GetAgentsClient();
        
        AgentThread thread = await agentClient.CreateThreadAsync();
        return thread.Id;
    }
    
    public async Task<IEnumerable<string>> RunCodeInterpreterTool(string assistantId, string threadId, string userInput)
    {
        var projectClient = new AIProjectClient(options.Value.ConnectionString, new DefaultAzureCredential());
        var agentClient = projectClient.GetAgentsClient();
        
        Agent agent = await agentClient.GetAgentAsync(assistantId);
        AgentThread thread = await agentClient.GetThreadAsync(threadId);
        
        await agentClient.CreateMessageAsync( threadId, role: MessageRole.User, content: userInput);
        
        ThreadRun run = await agentClient.CreateRunAsync(thread, agent);

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            run = await agentClient.GetRunAsync(thread.Id, run.Id);
        }
        while (run.Status == RunStatus.Queued
               || run.Status == RunStatus.InProgress
               || run.Status == RunStatus.RequiresAction);
        
        var messagesResponse = await agentClient.GetMessagesAsync(thread.Id, order: ListSortOrder.Descending);
        var result = new List<string>();

        foreach (var threadMessage in messagesResponse.Value.Data)
        {
            if(threadMessage.ContentItems.FirstOrDefault() is MessageTextContent messageTextContent) 
                result.Add(messageTextContent.Text);
        }

        return result;
    }
}