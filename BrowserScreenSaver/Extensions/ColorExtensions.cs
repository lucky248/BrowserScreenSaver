using System.Windows.Media;

namespace BrowserScreenSaver.Extensions
{
    internal static class ColorExtensions
    {
        public static Brush CreateBlendBrush(this Color baseColor, Color blendColor, double ammountRatio)
        {
            Color blended = new Color()
            {
                A = (byte)((1 - ammountRatio) * baseColor.A + blendColor.A * ammountRatio),
                B = (byte)((1 - ammountRatio) * baseColor.B + blendColor.B * ammountRatio),
                G = (byte)((1 - ammountRatio) * baseColor.G + blendColor.G * ammountRatio),
                R = (byte)((1 - ammountRatio) * baseColor.R + blendColor.R * ammountRatio),
            };
            return new SolidColorBrush(blended);
        }

        public static int GetColorCode(this Color color)
        {
            return (int)((((uint)color.A) << 24) + (int)(((uint)color.R) << 16) + (int)(((uint)color.G) << 8) + color.B);
        }
    }
}
