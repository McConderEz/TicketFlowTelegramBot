using System.Text;

namespace TelegramBotCode418Service.Features.HttpHandlers;

public class HttpPostHandlers
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HttpPostHandlers> _logger;

    public HttpPostHandlers(
        IConfiguration configuration,
        ILogger<HttpPostHandlers> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task SendPostRequestAsync(string data, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = new HttpClient();

        var url = _configuration.GetConnectionString("Url");
        var byteArray = Encoding.UTF8.GetBytes(data);
        
        using var content = new ByteArrayContent(byteArray);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        
        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Content = content;

        var response = await httpClient.SendAsync(request,cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation($"Response: {responseData}");
        }
        else
        {
            _logger.LogError($"Error: {response.StatusCode}");
        }
    }
}