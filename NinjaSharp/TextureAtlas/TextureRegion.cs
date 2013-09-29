using Microsoft.Xna.Framework;

namespace ThirdPartyNinjas.NinjaSharp
{
	public class TextureRegion
	{
		public Rectangle Bounds { get; set; } // pixel position/dimension of the texture (trimmed)
        public Rectangle Input { get; set; }  // (x, y) is amount trimmed from top left, (w, y) are the input width and height

		public Vector2 OriginTopLeft { get; set; }
		public Vector2 OriginCenter { get; set; }
		public Vector2 OriginBottomRight { get; set; }

		public bool Rotated { get; set; }
	}
}
