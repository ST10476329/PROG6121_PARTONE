namespace RaceDayWeb.Services;

public class ImageUrlService
{
    private readonly string _apiBase;

    public ImageUrlService(IConfiguration config)
    {
        _apiBase = config["ApiBaseUrl"].TrimEnd('/');
    }

    // the address to use in an <img> for an event banner
    public string Banner(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "/images/default-banner.svg";

        return _apiBase + path;
    }

    // the address to use in an <img> for a profile picture
    public string Profile(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "/images/default-avatar.svg";

        return _apiBase + path;
    }
}