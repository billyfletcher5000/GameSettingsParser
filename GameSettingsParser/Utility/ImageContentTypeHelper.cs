using System.IO;
using System.Windows.Media.Imaging;

namespace GameSettingsParser.Utility
{

    public static class ImageContentTypeHelper
    {
        public static string GetImageContentType(string imagePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(imagePath);

            using var stream = File.OpenRead(imagePath);

            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            var mimeTypes = decoder.CodecInfo?.MimeTypes;

            var contentType = mimeTypes?
                .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault(mimeType => mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                return contentType;
            }

            return GetFallbackContentTypeFromExtension(imagePath);
        }

        private static string GetFallbackContentTypeFromExtension(string imagePath)
        {
            return Path.GetExtension(imagePath).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" or ".jpe" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" or ".dib" => "image/bmp",
                ".tif" or ".tiff" => "image/tiff",
                ".ico" => "image/x-icon",
                ".wdp" or ".jxr" or ".hdp" => "image/vnd.ms-photo",
                _ => "application/octet-stream"
            };
        }
    }
}