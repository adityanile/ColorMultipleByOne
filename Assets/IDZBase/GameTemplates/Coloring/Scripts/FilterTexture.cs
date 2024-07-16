using UnityEngine;

namespace IDZBase.Core.GameTemplates.Coloring
{
    public class FilterTexture
    {
        private readonly Texture2D _textureCopy;
        
        public FilterTexture(Texture2D texture2D, Color32 color, int tolerance)
        {
            _textureCopy = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, false);
            var copyColors = _textureCopy.GetPixels32();
            var colors = texture2D.GetPixels32();

            for (var i = 0; i < colors.Length; i++)
            {
                copyColors[i] = colors[i].r > color.r - tolerance && colors[i].r < color.r + tolerance &&
                                colors[i].g > color.g - tolerance && colors[i].g < color.g + tolerance &&
                                colors[i].b > color.b - tolerance && colors[i].b < color.b + tolerance &&
                                colors[i].a > color.a - tolerance && colors[i].a < color.a + tolerance
                    ? colors[i]
                    : Color.clear;
            }
            
            _textureCopy.SetPixels32(copyColors);
            _textureCopy.Apply();
        }

        public Texture2D GetTexture()
        {
            return _textureCopy;
        }

        public void Dispose()
        {
            Object.Destroy(_textureCopy);
        }
    }
}