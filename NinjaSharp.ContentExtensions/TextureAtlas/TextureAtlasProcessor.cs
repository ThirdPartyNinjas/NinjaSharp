using Microsoft.Xna.Framework.Content.Pipeline;

namespace ThirdPartyNinjas.NinjaSharp.ContentExtensions
{
	[ContentProcessor(DisplayName = "Texture Atlas Processor - ThirdPartyNinjas")]
	public class TextureAtlasProcessor : ContentProcessor<TextureAtlasContent, TextureAtlasContent>
	{
		public override TextureAtlasContent Process(TextureAtlasContent input, ContentProcessorContext context)
		{
			return input;
		}
	}
}
