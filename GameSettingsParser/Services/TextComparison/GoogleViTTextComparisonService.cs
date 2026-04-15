using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GameSettingsParser.Utility;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace GameSettingsParser.Services.TextComparison
{
    public class GoogleViTTextComparisonService : MLTextComparisonService
    {
        private static readonly string ModelFolderPath = @"./onnx-models/google-vit-base-patch16-224";
        private static readonly string ModelFileName = "model.onnx";
        protected override string ModelPath => $"{ModelFolderPath}/{ModelFileName}";

        protected override int TargetSize => 224;
        protected override float[] Mean => [0.5f, 0.5f, 0.5f];
        protected override float[] Std => [0.5f, 0.5f, 0.5f];
    }
}