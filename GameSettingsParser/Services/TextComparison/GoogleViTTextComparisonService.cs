using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace GameSettingsParser.Services.TextComparison
{
    public class GoogleViTTextComparisonService : ITextComparisonService
    {
        private static readonly string ModelFolderPath = @"/onnx-models/google-vit-base-patch16-224";
        private static readonly string ModelFileName = "model.onnx";
        private static readonly string ConfigFileName = "config.json";
        
        private static string ModelPath => $"{ModelFolderPath}/{ModelFileName}";
        private static string ConfigPath => $"{ModelFolderPath}/{ConfigFileName}";
        
        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB)
        {
            var session = new InferenceSession(ModelPath);
            
            var imageAFeatures = ExtractFeatures(session, imageA);
            var imageBFeatures = ExtractFeatures(session, imageB);

            return CalculateCosineSimilarity(imageAFeatures, imageBFeatures);
        }

        private static float[] ExtractFeatures(InferenceSession session, Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb); //

            byte[] pixelBytes;
            try
            {
                IntPtr ptr = bmpData.Scan0;
                int bytes = Math.Abs(bmpData.Stride) * height;
                pixelBytes = new byte[bytes];

                Marshal.Copy(ptr, pixelBytes, 0, bytes);
            }
            finally
            {
                image.UnlockBits(bmpData);
            }

            const int channels = 3;
            var tensor = new DenseTensor<float>(new[] { 1, channels, height, width });

            for (int y = 0; y < height; y++)
            {
                int rowStartInBytes = y * bmpData.Stride; //

                for (int x = 0; x < width; x++)
                {
                    int pixelOffset = rowStartInBytes + (x * channels);

                    float b = pixelBytes[pixelOffset] / 255.0f;
                    float g = pixelBytes[pixelOffset + 1] / 255.0f;
                    float r = pixelBytes[pixelOffset + 2] / 255.0f;

                    tensor[0, 0, y, x] = r;
                    tensor[0, 1, y, x] = g;
                    tensor[0, 2, y, x] = b;
                }
            }

            var inputName = session.InputMetadata.Keys.First();
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, tensor) };

            using var results = session.Run(inputs);
            return results.First().AsTensor<float>().ToArray();
        }
        
        private static double CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                normA += Math.Pow(vecA[i], 2);
                normB += Math.Pow(vecB[i], 2);
            }
            
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}