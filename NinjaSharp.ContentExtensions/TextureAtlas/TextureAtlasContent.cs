using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace ThirdPartyNinjas.NinjaSharp.ContentExtensions
{
	public class TextureAtlasContent
	{
		public Dictionary<string, TextureRegionContent> Regions { get; private set; }

		public TextureAtlasContent()
		{
		}

		public TextureAtlasContent(string fileName)
		{
			if (Path.GetExtension(fileName).Equals(".json"))
				InitializeFromJson(fileName);
			else
				throw new PipelineException("Unknown extension. TextureAtlas cannot be loaded from the file " + fileName);
		}

		private void InitializeFromJson(string fileName)
		{
			Regions = new Dictionary<string, TextureRegionContent>();

			System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
			string jsonText = sr.ReadToEnd();
			sr.Close();
			sr.Dispose();

			JavaScriptSerializer jss = new JavaScriptSerializer();
			TextureDictionaryData data = jss.Deserialize<TextureDictionaryData>(jsonText);

			foreach (var frame in data.frames)
			{
				TextureRegionContent textureRegion = new TextureRegionContent();

				textureRegion.Bounds = new Microsoft.Xna.Framework.Rectangle(frame.frame.x, frame.frame.y, frame.frame.w, frame.frame.h);
                textureRegion.Input = new Microsoft.Xna.Framework.Rectangle(frame.spriteSourceSize.x, frame.spriteSourceSize.y, frame.sourceSize.w, frame.sourceSize.h);

				textureRegion.OriginTopLeft = new Vector2(-frame.spriteSourceSize.x, -frame.spriteSourceSize.y);
				textureRegion.OriginCenter = new Vector2(((frame.sourceSize.w / 2.0f) - (frame.spriteSourceSize.x)), ((frame.sourceSize.h / 2.0f) - (frame.spriteSourceSize.y)));
				textureRegion.OriginBottomRight = new Vector2((frame.sourceSize.w - (frame.spriteSourceSize.x)), (frame.sourceSize.h - (frame.spriteSourceSize.y)));

                textureRegion.Rotated = frame.rotated;

				Regions[Path.GetFileNameWithoutExtension(frame.filename)] = textureRegion;
			}
		}

		internal class Rectangle
		{
			public int x { get; set; }
			public int y { get; set; }
			public int w { get; set; }
			public int h { get; set; }
		}

		internal class Dimensions
		{
			public int w { get; set; }
			public int h { get; set; }
		}

		internal class Frame
		{
			public string filename { get; set; }
			public Rectangle frame { get; set; }
			public bool rotated { get; set; }
			public bool trimmed { get; set; }
			public Rectangle spriteSourceSize { get; set; }
			public Dimensions sourceSize { get; set; }
		}

		internal class Meta
		{
			public string app { get; set; }
			public string version { get; set; }
			public string image { get; set; }
			public string format { get; set; }
			public Dimensions size { get; set; }
			public string scale { get; set; }
			public string smartupdate { get; set; }
		}

		internal class TextureDictionaryData
		{
			public Frame[] frames { get; set; }
			public Meta meta { get; set; }
		}
	}
}

