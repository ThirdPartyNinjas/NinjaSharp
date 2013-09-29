using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;

using Spine;

namespace ThirdPartyNinjas.NinjaSharp.Spine
{
	public class AtlasReader : ContentTypeReader<Atlas>
	{
		protected override Atlas Read(ContentReader input, Atlas existingInstance)
		{
			List<AtlasPage> pages = input.ReadObject<List<AtlasPage>>();
			List<AtlasRegion> regions = input.ReadObject<List<AtlasRegion>>();

			return new Atlas(pages, regions);
		}
	}
}
