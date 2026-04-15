using System.Drawing;
using GameSettingsParser.Utility;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace GameSettingsParser.Services.TextComparison
{
    public abstract class MLTextComparisonService : ITextComparisonService
    {
        protected abstract string ModelPath { get; }
        protected abstract int TargetSize { get; }
        protected abstract float[] Mean { get; }
        protected abstract float[] Std { get; }
        
        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB)
        {
            var session = new InferenceSession(ModelPath);
            
            var imageAFeatures = ExtractFeatures(session, imageA);
            var imageBFeatures = ExtractFeatures(session, imageB);

            return CalculateCosineSimilarity(imageAFeatures, imageBFeatures);
        }

        protected virtual DenseTensor<float> CreateTensor(float[] data)
        {
            return new DenseTensor<float>(data, new[] { 1, 3, TargetSize, TargetSize });
        }

        protected virtual float[] RetrieveResult(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> onnxResult)
        {
            return onnxResult.First().AsTensor<float>().ToArray();
        }

        private float[] ExtractFeatures(InferenceSession session, Bitmap image)
        {
            MLPreprocessor preprocessor = new MLPreprocessor(TargetSize, Mean, Std);
            float[] data = preprocessor.Preprocess(image);
            var tensor = CreateTensor(data);

            var inputName = session.InputMetadata.Keys.First();
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, tensor) };
  
            using var results = session.Run(inputs);
            return RetrieveResult(results); 
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