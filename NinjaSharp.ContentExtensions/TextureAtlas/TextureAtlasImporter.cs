using Microsoft.Xna.Framework.Content.Pipeline;

namespace ThirdPartyNinjas.NinjaSharp.ContentExtensions
{
	[ContentImporter(".json", DisplayName = "Texture Atlas Importer - ThirdPartyNinjas", DefaultProcessor = "TextureAtlasProcessor")]
	public class TextureAtlasImporter : ContentImporter<TextureAtlasContent>
	{
		public override TextureAtlasContent Import(string fileName, ContentImporterContext context)
		{
			return new TextureAtlasContent(fileName);
		}
	}
}
