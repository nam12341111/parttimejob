using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion; // Added
using Microsoft.EntityFrameworkCore; // Added
using Microsoft.Extensions.Configuration; // Added
using PTJ.Application.Common;
using PTJ.Application.Services;
using PTJ.Infrastructure.AI.Plugins;
using PTJ.Infrastructure.Persistence;
using PTJ.Domain.Entities;

namespace PTJ.Infrastructure.Services;

public class AIChatService : IAIChatService
{
    private readonly IConfiguration _configuration;
    private readonly Kernel _kernel;
    private readonly AppDbContext _dbContext;
    private readonly JobSearchPlugin _jobSearchPlugin;
    private readonly IdentityPlugin _identityPlugin;
    private readonly ProfilePlugin _profilePlugin;
    private readonly JobDetailPlugin _jobDetailPlugin;

    public AIChatService(
        Kernel kernel,
        AppDbContext dbContext,
        JobSearchPlugin jobSearchPlugin,
        IdentityPlugin identityPlugin,
        ProfilePlugin profilePlugin,
        JobDetailPlugin jobDetailPlugin,
        IConfiguration configuration)
    {
        _kernel = kernel;
        _dbContext = dbContext;
        _jobSearchPlugin = jobSearchPlugin;
        _identityPlugin = identityPlugin;
        _profilePlugin = profilePlugin;
        _jobDetailPlugin = jobDetailPlugin;
        _configuration = configuration;
    }

    public async Task<Result> RestartSessionAsync(int userId, CancellationToken cancellationToken = default)
    {
        var activeSessions = await _dbContext.AIChatSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync(cancellationToken);

        if (activeSessions.Any())
        {
            foreach (var session in activeSessions)
            {
                session.IsActive = false;
                session.EndedAt = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.SuccessResult("Conversation restarted successfully.");
    }

    public async Task<Result<string>> ChatAsync(int userId, string userMessage, CancellationToken cancellationToken = default)
    {
        // 1. Join Plugins
        RegisterPlugins();

        try
        {
            // 2. Get active session
            var session = await GetOrCreateActiveSessionAsync(userId, cancellationToken);

            // 3. Build History Context
            var chatHistory = await BuildChatHistoryAsync(session.Id, userId, cancellationToken);

            // Add current user message
            chatHistory.AddUserMessage(userMessage);

            // 4. Setup Execution Settings
            var settings = new OpenAIPromptExecutionSettings()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.7,
                TopP = 0.9,
            };

            // 5. Invoke Chat Completion
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, _kernel, cancellationToken);

            var responseContent = result.Content ?? "";

            // 6. Save Messages to DB
            // Save User Message
            _dbContext.AIChatMessages.Add(new AIChatMessage
            {
                SessionId = session.Id,
                Role = "user",
                Content = userMessage,
                TokenCount = userMessage.Length / 3 // Rough estimate
            });

            // Save Assistant Message
            if (!string.IsNullOrEmpty(responseContent))
            {
                _dbContext.AIChatMessages.Add(new AIChatMessage
                {
                    SessionId = session.Id,
                    Role = "assistant",
                    Content = responseContent,
                    TokenCount = responseContent.Length / 3
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<string>.SuccessResult(responseContent);
        }
        catch (Exception ex)
        {
            return Result<string>.FailureResult($"AI processing failed: {ex.Message}. Make sure Ollama is running at http://localhost:11434");
        }
    }

    private void RegisterPlugins()
    {
        if (!_kernel.Plugins.Contains("JobSearch")) _kernel.Plugins.AddFromObject(_jobSearchPlugin, "JobSearch");
        if (!_kernel.Plugins.Contains("Identity")) _kernel.Plugins.AddFromObject(_identityPlugin, "Identity");
        if (!_kernel.Plugins.Contains("Profile")) _kernel.Plugins.AddFromObject(_profilePlugin, "Profile");
        if (!_kernel.Plugins.Contains("JobDetail")) _kernel.Plugins.AddFromObject(_jobDetailPlugin, "JobDetail");
    }

    private async Task<AIChatSession> GetOrCreateActiveSessionAsync(int userId, CancellationToken ct)
    {
        var session = await _dbContext.AIChatSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, ct);

        if (session == null)
        {
            session = new AIChatSession
            {
                UserId = userId,
                Title = "New Chat Conversation",
                IsActive = true
            };
            _dbContext.AIChatSessions.Add(session);
            await _dbContext.SaveChangesAsync(ct);
        }
        return session;
    }

    private async Task<ChatHistory> BuildChatHistoryAsync(int sessionId, int userId, CancellationToken ct)
    {
        // Limit: 20000 tokens (approx 60000 chars)
        const int MAX_CHARS = 60000;

        // Fetch last 50 messages
        var messages = await _dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(50) // Adjust if needed
            .ToListAsync(ct);

        int currentChars = 0;
        var selectedMessages = new List<AIChatMessage>();

        foreach (var msg in messages)
        {
            int len = msg.Content?.Length ?? 0;
            if (currentChars + len > MAX_CHARS) break;

            selectedMessages.Add(msg);
            currentChars += len;
        }

        selectedMessages.Reverse(); // Restore Chronological Order

        var history = new ChatHistory();

        // --- ADD SYSTEM PROMPT HERE ---
        var systemPrompt = _configuration["AI:OpenAI:SystemInstructions"];

        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            // Default Fallback
            systemPrompt = """
                You are a helpful AI assistant.
                """;
        }

        // Inject UserID into system prompt
        systemPrompt += $"\n\n[System Info]: The user you are chatting with has UserID: {userId}.";

        history.AddSystemMessage(systemPrompt);

        foreach (var msg in selectedMessages)
        {
            if (msg.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
                history.AddUserMessage(msg.Content);
            else if (msg.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase))
                history.AddAssistantMessage(msg.Content);
        }

        return history;
    }
}
