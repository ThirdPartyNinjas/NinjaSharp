using System.IO;

using Microsoft.Xna.Framework.Content;

using Spine;

namespace ThirdPartyNinjas.NinjaSharp.Spine
{
	public class SkeletonDataReader : ContentTypeReader<SkeletonData>
	{
		protected override SkeletonData Read(ContentReader input, SkeletonData existingInstance)
		{
			string atlasAssetName = input.ReadString();
			string skeletonDataName = input.ReadString();
			string skeletonJsonText = input.ReadString();

			Atlas atlas = input.ContentManager.Load<Atlas>(atlasAssetName);
			SkeletonJson skeletonJson = new SkeletonJson(atlas);

			SkeletonData skeletonData;

			using (TextReader textReader = new StringReader(skeletonJsonText))
			{
				skeletonData = skeletonJson.ReadSkeletonData(textReader);
			}

			skeletonData.Name = skeletonDataName;
			return skeletonData;
		}
	}
}
