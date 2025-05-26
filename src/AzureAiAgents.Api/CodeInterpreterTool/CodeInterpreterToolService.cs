using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace AzureAiAgents.Api.CodeInterpreterTool;

public class CodeInterpreterToolService(IOptions<AzureAiAgentSettings> options)
{
    
    public async Task<string> CreateAgentAsync(Stream fileStream, string fileName, string agentName, string instructions)
    {
        var agentClient = new PersistentAgentsClient(options.Value.Uri, new DefaultAzureCredential());
        List<ToolDefinition> tools = [ new CodeInterpreterToolDefinition() ];

        PersistentAgentFileInfo uploadAgentFile = await agentClient.Files.UploadFileAsync(fileStream, PersistentAgentFilePurpose.Agents, fileName);
        
        PersistentAgent agent = await agentClient.Administration.CreateAgentAsync(options.Value.Model,
            name: agentName,
            instructions: instructions,
            tools: tools,
            toolResources: new ToolResources
            {
                CodeInterpreter = new CodeInterpreterToolResource
                {
                    FileIds = { uploadAgentFile.Id }
                }
            });
        return agent.Id;
    }
    
    public async Task<string> CreateAgentAsync(string agentName, string instructions)
    {
        var agentClient = new PersistentAgentsClient(options.Value.Uri, new DefaultAzureCredential());
        List<ToolDefinition> tools = [ new CodeInterpreterToolDefinition() ];
        PersistentAgent agent = await agentClient.Administration.CreateAgentAsync(options.Value.Model,
            name: agentName,
            instructions: instructions,
            tools: tools);
        return agent.Id;
    }

    public async Task<string> CreateThreadAsync()
    {
        var projectClient = new AIProjectClient(new Uri(options.Value.Uri), new DefaultAzureCredential());
        var agentClient = projectClient.GetPersistentAgentsClient();

        PersistentAgentThread thread = await agentClient.Threads.CreateThreadAsync();
        return thread.Id;
    }

    public async Task<string> CreateThreadAsync(Stream fileStream, string fileName)
    {
        var projectClient = new AIProjectClient(new Uri(options.Value.Uri), new DefaultAzureCredential());
        var agentClient = projectClient.GetPersistentAgentsClient();

        PersistentAgentFileInfo uploadAgentFile = await agentClient.Files.UploadFileAsync(fileStream, PersistentAgentFilePurpose.Agents, fileName);
        var tools = new ToolResources
        {
            CodeInterpreter = new CodeInterpreterToolResource
            {
                FileIds = {  uploadAgentFile.Id }
            }
        };
        
        PersistentAgentThread thread = await agentClient.Threads.CreateThreadAsync(toolResources: tools);
        return thread.Id;
    }
    
    public async Task<IEnumerable<string>> RunAsync(string assistantId, string threadId, string userInput)
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
    
    public async Task<Stream?> RunWithAttachementsAsync(string assistantId, string threadId, string userInput)
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
        await foreach (var threadMessage in messagesResponse)
        {
            if (threadMessage.Attachments.Count <= 0) continue;
            PersistentAgentFileInfo fileInfo = await agentClient.Files.GetFileAsync(threadMessage.Attachments[0].FileId);
            if (fileInfo.Purpose != PersistentAgentFilePurpose.AgentsOutput) continue;
            
            BinaryData content = await agentClient.Files.GetFileContentAsync(threadMessage.Attachments[0].FileId);
            MemoryStream memoryStream = new();
            await content.ToStream().CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        return null;
    }
}