namespace RaceDayApi.Helpers;

public static class ImageHelper
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxBytes = 2 * 1024 * 1024;   // 2 MB

    // returns an error message, or null if the file is fine
    public static string? CheckImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return "Please choose an image file.";

        if (file.Length > MaxBytes)
            return "The image must be 2 MB or smaller.";

        string extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            return "Only .jpg, .jpeg, .png or .webp images are allowed.";

        if (!file.ContentType.StartsWith("image/"))
            return "That file is not an image.";

        return null;
    }

    // saves the image in wwwroot/uploads/{folder} and returns its URL path, e.g. /uploads/banners/abc.jpg
    public static async Task<string> SaveImage(IFormFile file, string folder, string webRootPath)
    {
        string extension = Path.GetExtension(file.FileName).ToLower();

        // a random file name, so one upload can never overwrite another
        string fileName = Guid.NewGuid().ToString() + extension;

        string folderPath = Path.Combine(webRootPath, "uploads", folder);
        Directory.CreateDirectory(folderPath);

        string fullPath = Path.Combine(folderPath, fileName);
        using (FileStream stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return "/uploads/" + folder + "/" + fileName;
    }

    // deletes an old image file if there is one
    public static void DeleteImage(string? url, string webRootPath)
    {
        if (string.IsNullOrEmpty(url))
            return;

        string relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        string fullPath = Path.Combine(webRootPath, relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}