using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using Spine;

namespace ThirdPartyNinjas.NinjaSharp.ContentExtensions.Spine
{
	public class SkeletonDataContent
	{
		public string atlasAssetName;
		public string skeletonDataName;
		public string skeletonJsonText;
	}

	[ContentImporter(".json", DisplayName = "SkeletonData Importer - Spine", DefaultProcessor = "SkeletonDataProcessor")]
	public class SkeletonDataImporter : ContentImporter<string>
	{
		public override string Import(string filename, ContentImporterContext context)
		{
			return filename;
		}
	}

	[ContentProcessor(DisplayName = "SkeletonData Processor - Spine")]
	public class SkeletonDataProcessor : ContentProcessor<string, SkeletonDataContent>
	{
		public string AtlasAssetName { get; set; }

		public override SkeletonDataContent Process(string filename, ContentProcessorContext context)
		{
			SkeletonDataContent skeletonDataContent = new SkeletonDataContent();

			skeletonDataContent.atlasAssetName = AtlasAssetName;
			skeletonDataContent.skeletonDataName = Path.GetFileNameWithoutExtension(filename);
			skeletonDataContent.skeletonJsonText = File.ReadAllText(filename);

			return skeletonDataContent;
		}
	}

	[ContentTypeWriter]
	public class SkeletonDataWriter : ContentTypeWriter<SkeletonDataContent>
	{
		protected override void Write(ContentWriter output, SkeletonDataContent value)
		{
			output.Write(value.atlasAssetName);
			output.Write(value.skeletonDataName);
			output.Write(value.skeletonJsonText);
		}

		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "ThirdPartyNinjas.NinjaSharp.Spine.SkeletonDataReader, NinjaSharp";
		}
	}
}
