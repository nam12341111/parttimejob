using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

Console.WriteLine("=== SignalR Chat Test Client ===\n");

// Configuration
var apiUrl = "http://localhost:5000";
var hubUrl = $"{apiUrl}/hubs/chat";

// 1. Login
Console.Write("Email: ");
var email = Console.ReadLine();
Console.Write("Password: ");
var password = Console.ReadLine();

using var httpClient = new HttpClient();
var loginResponse = await httpClient.PostAsJsonAsync($"{apiUrl}/api/auth/login", new
{
    email,
    password
});

var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();

if (loginResult?.Success != true || string.IsNullOrEmpty(loginResult.Data?.AccessToken))
{
    Console.WriteLine("Login failed!");
    return;
}

var token = loginResult.Data.AccessToken;
Console.WriteLine($"\n✅ Login successful! Token: {token[..50]}...\n");

// 2. Connect to SignalR
var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl, options =>
    {
        options.AccessTokenProvider = () => Task.FromResult<string?>(token);
    })
    .WithAutomaticReconnect()
    .Build();

// Event handlers
connection.On<ChatMessage>("ReceiveMessage", (message) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n📨 [{message.SenderName}]: {message.Content}");
    Console.WriteLine($"   Time: {message.CreatedAt:HH:mm:ss}");
    Console.ResetColor();
});

connection.On<int>("MessagesMarkedAsRead", (conversationId) =>
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\n✅ Messages marked as read in conversation {conversationId}");
    Console.ResetColor();
});

connection.On<int, bool>("UserTyping", (userId, isTyping) =>
{
    if (isTyping)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n✍️ User {userId} is typing...");
        Console.ResetColor();
    }
});

connection.Closed += async (error) =>
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n❌ Connection closed: {error?.Message}");
    Console.ResetColor();
    await Task.Delay(5000);
};

try
{
    await connection.StartAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✅ Connected to SignalR hub!\n");
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
    return;
}

// 3. Chat loop
Console.WriteLine("Commands:");
Console.WriteLine("  /send <recipientId> <message> - Send message");
Console.WriteLine("  /typing <conversationId> - Toggle typing");
Console.WriteLine("  /exit - Exit\n");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input)) continue;

    var parts = input.Split(' ', 3);
    var command = parts[0].ToLower();

    try
    {
        switch (command)
        {
            case "/send":
                if (parts.Length < 3)
                {
                    Console.WriteLine("Usage: /send <recipientId> <message>");
                    break;
                }

                await connection.InvokeAsync("SendMessage", new
                {
                    RecipientId = int.Parse(parts[1]),
                    Content = parts[2]
                });
                Console.WriteLine("✅ Message sent");
                break;

            case "/typing":
                if (parts.Length < 2)
                {
                    Console.WriteLine("Usage: /typing <conversationId>");
                    break;
                }

                await connection.InvokeAsync("UpdateTyping", int.Parse(parts[1]), true);
                await Task.Delay(2000);
                await connection.InvokeAsync("UpdateTyping", int.Parse(parts[1]), false);
                break;

            case "/exit":
                await connection.StopAsync();
                return;

            default:
                Console.WriteLine("Unknown command");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}

// DTOs
public class LoginResult
{
    public bool Success { get; set; }
    public LoginData? Data { get; set; }
}

public class LoginData
{
    public string? AccessToken { get; set; }
}

public class ChatMessage
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
