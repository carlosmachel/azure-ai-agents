using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace AzureAiAgents.Api.CodeInterpreterTool;

public class CodeInterpreterToolService(IOptions<AzureAiAgentSettings> options)
{
    
    public async Task<string> CreateAgentWithCodeInterpreterTool(Stream fileStream, string fileName)
    {
        var agentClient = new PersistentAgentsClient(options.Value.Uri, new DefaultAzureCredential());
        List<ToolDefinition> tools = [ new CodeInterpreterToolDefinition() ];

        PersistentAgentFileInfo uploadAgentFile = await agentClient.Files.UploadFileAsync(fileStream, PersistentAgentFilePurpose.Agents, fileName);
        
        PersistentAgent agent = await agentClient.Administration.CreateAgentAsync(options.Value.Model,
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
        var projectClient = new AIProjectClient(new Uri(options.Value.Uri), new DefaultAzureCredential());
        var agentClient = projectClient.GetPersistentAgentsClient();
        
        PersistentAgentThread thread = await agentClient.Threads.CreateThreadAsync();
        return thread.Id;
    }
    
    public async Task<IEnumerable<string>> RunCodeInterpreterTool(string assistantId, string threadId, string userInput)
    {
        var projectClient = new AIProjectClient(new Uri(options.Value.Uri), new DefaultAzureCredential());
        var agentClient = projectClient.GetPersistentAgentsClient();
        
        PersistentAgent agent = await agentClient.Administration.GetAgentAsync(assistantId);
        PersistentAgentThread thread = await agentClient.Threads.GetThreadAsync(threadId);
        
        await agentClient.Messages.CreateMessageAsync( threadId, role: MessageRole.User, content: userInput);
        
        ThreadRun run = await agentClient.Runs.CreateRunAsync(thread, agent);

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            run = await agentClient.Runs.GetRunAsync(thread.Id, run.Id);
        }
        while (run.Status == RunStatus.Queued
               || run.Status == RunStatus.InProgress
               || run.Status == RunStatus.RequiresAction);
        
        var messagesResponse = agentClient.Messages.GetMessagesAsync(thread.Id, order: ListSortOrder.Descending);
        var result = new List<string>();

        await foreach (var threadMessage in messagesResponse) 
        {
            if(threadMessage.ContentItems.FirstOrDefault() is MessageTextContent messageTextContent) 
                result.Add(messageTextContent.Text);
        }

        return result;
    }
}