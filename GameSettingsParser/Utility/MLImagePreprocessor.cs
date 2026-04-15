using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Point = SixLabors.ImageSharp.Point;
using Size = SixLabors.ImageSharp.Size;

namespace GameSettingsParser.Utility
{
    public class MLPreprocessor
    {
        public int TargetSize { get; set; } = 224;

        public float[] Mean { get; set; } = [0.485f, 0.456f, 0.406f];
        public float[] Std { get; set; } = [0.229f, 0.224f, 0.225f];

        public MLPreprocessor()
        {
        }

        public MLPreprocessor(int targetSize, float[] mean, float[] std)
        {
            TargetSize = targetSize;
            Mean = mean;
            Std = std;
        }

        public float[] Preprocess(Bitmap bitmap)
        {
            using var image = BitmapToImage(bitmap);
            using var square = ResizeAndPadToSquare(image, TargetSize);
            return ExtractRgbFloatArray(square);
        }

        private Image<Rgba32> BitmapToImage(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            return Image.Load<Rgba32>(ms);
        }

        private Image<Rgba32> ResizeAndPadToSquare(Image<Rgba32> source, int targetSize)
        {
            int srcW = source.Width;
            int srcH = source.Height;

            double scale = Math.Min((double)targetSize / srcW, (double)targetSize / srcH);
            int resizedW = Math.Max(1, (int)Math.Round(srcW * scale));
            int resizedH = Math.Max(1, (int)Math.Round(srcH * scale));

            var resized = source.Clone(ctx =>
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(resizedW, resizedH),
                    Mode = ResizeMode.Stretch,
                    Sampler = KnownResamplers.Lanczos3
                }));

            var square = new Image<Rgba32>(targetSize, targetSize);

            int offsetX = (targetSize - resizedW) / 2;
            int offsetY = (targetSize - resizedH) / 2;

            square.Mutate(ctx =>
            {
                ctx.DrawImage(resized, new Point(offsetX, offsetY), 1f);
            });

            resized.Dispose();
            return square;
        }

        private float[] ExtractRgbFloatArray(Image<Rgba32> image)
        {
            int w = image.Width;
            int h = image.Height;
            int plane = w * h;
            float[] result = new float[3 * plane];

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < h; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        Rgba32 pixel = row[x];
                        int p = y * w + x;

                        result[p] = pixel.R / 255f;
                        result[plane + p] = pixel.G / 255f;
                        result[2 * plane + p] = pixel.B / 255f;
                    }
                }
            });

            NormalizeInPlace(result, plane);
            return result;
        }

        private void NormalizeInPlace(float[] pixels, int plane)
        {
            for (int i = 0; i < plane; i++)
            {
                pixels[i] = (pixels[i] - Mean[0]) / Std[0];
                pixels[plane + i] = (pixels[plane + i] - Mean[1]) / Std[1];
                pixels[2 * plane + i] = (pixels[2 * plane + i] - Mean[2]) / Std[2];
            }
        }
    }
}