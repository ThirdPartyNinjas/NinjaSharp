using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;

namespace ThirdPartyNinjas.NinjaSharp
{
	public class TextureAtlasReader : ContentTypeReader<TextureAtlas>
	{
		protected override TextureAtlas Read(ContentReader input, TextureAtlas existingInstance)
		{
			Dictionary<string, TextureRegion> regions = input.ReadObject<Dictionary<string, TextureRegion>>();
			return new TextureAtlas(regions);
		}
	}
}
