namespace RaceDayApi.Dtos;

/// <summary>The address of an uploaded image.</summary>
public class ImageUploadResponseDto
{
    /// <summary>Path of the saved image on the API, e.g. /uploads/banners/abc.jpg. Put the API's address in front of it to show the image.</summary>
    public string Url { get; set; } = "";
}