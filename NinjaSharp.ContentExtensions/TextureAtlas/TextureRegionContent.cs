using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ThirdPartyNinjas.NinjaSharp.ContentExtensions
{
    [ContentSerializerRuntimeType("ThirdPartyNinjas.NinjaSharp.TextureRegion, NinjaSharp")]
	public class TextureRegionContent
	{
		public Rectangle Bounds { get; set; }
        public Rectangle Input { get; set; }

		public Vector2 OriginTopLeft { get; set; }
		public Vector2 OriginCenter { get; set; }
		public Vector2 OriginBottomRight { get; set; }

		public bool Rotated { get; set; }
	}
}
