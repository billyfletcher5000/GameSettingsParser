using System.Windows;
using System.Windows.Media;

namespace GameSettingsParser.Utility
{
    public static class OverlayTransformHelper
    {
        public static TransformGroup CreateImageToViewTransform(
            Size imagePixelSize,
            Size viewSize,
            Stretch stretch)
        {
            if (imagePixelSize.Width <= 0 || imagePixelSize.Height <= 0)
                return new TransformGroup();

            double scaleX = viewSize.Width / imagePixelSize.Width;
            double scaleY = viewSize.Height / imagePixelSize.Height;

            double finalScaleX;
            double finalScaleY;
            double offsetX = 0;
            double offsetY = 0;

            switch (stretch)
            {
                case Stretch.Fill:
                    finalScaleX = scaleX;
                    finalScaleY = scaleY;
                    break;

                case Stretch.Uniform:
                    {
                        double scale = Math.Min(scaleX, scaleY);
                        finalScaleX = scale;
                        finalScaleY = scale;

                        double displayWidth = imagePixelSize.Width * scale;
                        double displayHeight = imagePixelSize.Height * scale;

                        offsetX = (viewSize.Width - displayWidth) / 2.0;
                        offsetY = (viewSize.Height - displayHeight) / 2.0;
                        break;
                    }

                case Stretch.UniformToFill:
                    {
                        double scale = Math.Max(scaleX, scaleY);
                        finalScaleX = scale;
                        finalScaleY = scale;

                        double displayWidth = imagePixelSize.Width * scale;
                        double displayHeight = imagePixelSize.Height * scale;

                        offsetX = (viewSize.Width - displayWidth) / 2.0;
                        offsetY = (viewSize.Height - displayHeight) / 2.0;
                        break;
                    }

                default:
                    finalScaleX = scaleX;
                    finalScaleY = scaleY;
                    break;
            }

            var transform = new TransformGroup();
            transform.Children.Add(new ScaleTransform(finalScaleX, finalScaleY));
            transform.Children.Add(new TranslateTransform(offsetX, offsetY));
            return transform;
        }
    }
}