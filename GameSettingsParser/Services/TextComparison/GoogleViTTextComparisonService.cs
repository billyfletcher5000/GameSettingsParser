using GameSettingsParser.Attributes;
using Size = System.Drawing.Size;

namespace GameSettingsParser.Services.TextComparison
{
    [SwitchableService(nameof(GoogleViTTextComparisonService), "Google ViT Feature Extraction")]
    public class GoogleViTTextComparisonService : MLTextComparisonService
    {
        private const string ModelFolderPath = @"./onnx-models/google-vit-base-patch16-224";
        private const string ModelFileName = "model.onnx";
        
        protected override string ModelPath => $"{ModelFolderPath}/{ModelFileName}";
        protected override Size TargetSize => new(224, 224);
        protected override float[] Mean => [0.5f, 0.5f, 0.5f];
        protected override float[] Std => [0.5f, 0.5f, 0.5f];
    }
}