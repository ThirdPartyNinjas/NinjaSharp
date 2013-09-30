using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace ThirdPartyNinjas.NinjaSharp.ContentExtensions
{
	[ContentTypeWriter]
	public class TextureAtlasWriter : ContentTypeWriter<TextureAtlasContent>
	{
		protected override void Write(ContentWriter output, TextureAtlasContent value)
		{
			output.WriteObject(value.Regions);
		}

		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
            return "ThirdPartyNinjas.NinjaSharp.TextureAtlasReader, NinjaSharp";
		}
	}
}
