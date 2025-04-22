using UnityEngine;

namespace Infrastructure.Extensions
{
    public static class ColorExtensions 
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}