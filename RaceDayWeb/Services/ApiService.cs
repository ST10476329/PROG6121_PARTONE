using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDayWeb.Services;

// what comes back from every API call
public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Body { get; set; }

    // only filled in when the API starts a session (login)
    public string SessionCookie { get; set; }

    public bool IsSuccess
    {
        get { return StatusCode >= 200 && StatusCode < 300; }
    }
}

public class ApiService
{
    private readonly IHttpClientFactory _factory;

    // the API sends camelCase JSON, so ignore capital letters when reading it
    public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public ApiService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    // sends a normal (JSON) request to the API
    public async Task<ApiResponse> SendAsync(HttpMethod method, string url, object body, string sessionCookie)
    {
        HttpRequestMessage request = new HttpRequestMessage(method, url);

        if (sessionCookie != null)
        {
            request.Headers.Add("Cookie", sessionCookie);
        }

        if (body != null)
        {
            string json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await Execute(request);
    }

    // sends a file (like a banner image) to the API
    public async Task<ApiResponse> SendFileAsync(string url, IFormFile file, string sessionCookie)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

        if (sessionCookie != null)
        {
            request.Headers.Add("Cookie", sessionCookie);
        }

        string contentType = file.ContentType;
        if (string.IsNullOrEmpty(contentType))
        {
            contentType = "application/octet-stream";
        }

        StreamContent fileContent = new StreamContent(file.OpenReadStream());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(fileContent, "file", file.FileName);   // "file" matches the API's parameter name
        request.Content = form;

        return await Execute(request);
    }

    // does the actual sending and turns the answer into an ApiResponse
    private async Task<ApiResponse> Execute(HttpRequestMessage request)
    {
        HttpClient client = _factory.CreateClient("RaceDayApi");

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            // the API is not running or can't be reached
            ApiResponse failed = new ApiResponse();
            failed.StatusCode = 503;
            failed.Body = "{\"message\":\"Could not reach the API. Check that it is running.\"}";
            return failed;
        }

        ApiResponse result = new ApiResponse();
        result.StatusCode = (int)response.StatusCode;
        result.Body = await response.Content.ReadAsStringAsync();

        // if the API started a session, its cookie is in the Set-Cookie header
        IEnumerable<string> cookies;
        if (response.Headers.TryGetValues("Set-Cookie", out cookies))
        {
            foreach (string cookie in cookies)
            {
                if (cookie.StartsWith(".RaceDayWeb.Session="))
                {
                    result.SessionCookie = cookie.Split(';')[0];
                }
            }
        }

        return result;
    }

    // finds the error text to show the user
    public string GetErrorMessage(ApiResponse response)
    {
        try
        {
            using (JsonDocument doc = JsonDocument.Parse(response.Body))
            {
                JsonElement message;
                if (doc.RootElement.TryGetProperty("message", out message))
                {
                    return message.GetString();
                }

                JsonElement errors;
                if (doc.RootElement.TryGetProperty("errors", out errors))
                {
                    return "Some of the details are not valid. Please check them and try again.";
                }
            }
        }
        catch (Exception)
        {
            // if the body was not json, he message below will be used
        }

        return "Something went wrong (error " + response.StatusCode + "). Please try again.";
    }
}