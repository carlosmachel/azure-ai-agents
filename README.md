# Azure AI Agents API

An open-source API project that provides Azure AI agent capabilities with a Code Interpreter Tool implementation.

## 🚀 Features

- Azure AI Agent integration
- Code Interpreter Tool
- OpenAPI/Swagger documentation
- HTTPS redirection
- Development environment tooling

## 📋 Prerequisites

- .NET 9.0 SDK or later
- An Azure account with appropriate permissions
- IDE (recommended: Visual Studio 2022 or JetBrains Rider)

## ⚙️ Configuration

The project requires Azure AI Agent settings to be configured. Add the following to your `appsettings.json`:

````
  "AzureAiAgentSettings" : {
    "Uri": "",
    "Model": ""
  }
````

## 🔧 Installation

1. Clone the repository

```bash
git clone [https://github.com/yourusername/azure-ai-agents-api.git](https://github.com/carlosmachel/azure-ai-agents-api.git)
```

2. Navigate to the project directory
```bash
cd azure-ai-agents-api
```

3. Restore dependencies
```bash
dotnet restore
```

4. Build the project
````bash
dotnet build
````

## 🚀 Running the Application

To run the application locally:

```bash
dotnet run
```

In development mode, you can access:
- Scalar API Reference at `/scalar`

## 🔒 Security

Make sure to:
- Never commit sensitive configuration values
- Use user secrets or environment variables for local development
- Properly configure Azure credentials in production

## 🤝 Contributing

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📬 Contact

Project Link: [https://github.com/carlosmachel/azure-ai-agents-api](https://github.com/carlosmachel/azure-ai-agents-api)

## 🙏 Acknowledgments

- Azure AI team for providing the underlying capabilities
- The .NET community
- All contributors to this project
