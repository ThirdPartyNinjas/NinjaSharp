using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;
using System.Xml.Linq;

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using Spine;

namespace ThirdPartyNinjas.NinjaSharp.ContentExtensions.Spine
{
	public class AtlasContent
	{
		public List<AtlasPage> pages = new List<AtlasPage>();
		public List<AtlasRegion> regions = new List<AtlasRegion>();
	}

	[ContentImporter(".atlas", ".json", DisplayName = "Atlas Importer - Spine", DefaultProcessor = "AtlasProcessor")]
	public class AtlasImporter : ContentImporter<string>
	{
		public override string Import(string filename, ContentImporterContext context)
		{
			return filename;
		}
	}

	[ContentProcessor(DisplayName = "Atlas Processor - Spine")]
	public class AtlasProcessor : ContentProcessor<string, AtlasContent>
	{
		// todo: Add processor parameters (including the ones to pass to TextureProcessor)

		[DisplayName("Texture Folder")]
		[Description("The folder to look in for the texture to go along with this atlas. Looks in the same folder as the atlas by default.")]
		public string TextureFolder { get; set; }

		public override AtlasContent Process(string filename, ContentProcessorContext context)
		{
			this.context = context;

			if (Path.GetExtension(filename).Equals(".json"))
				return LoadFromJson(filename);
			else if (Path.GetExtension(filename).Equals(".atlas"))
				return LoadFromAtlas(filename);
			else
				throw new PipelineException("AtlasProcessor.Process failed - Unknown file extension. (Supports: atlas, json)");
		}

		AtlasContent LoadFromJson(string filename)
		{
			System.IO.StreamReader sr = new System.IO.StreamReader(filename);
			string jsonText = sr.ReadToEnd();
			sr.Close();
			sr.Dispose();

			JavaScriptSerializer jss = new JavaScriptSerializer();
			TextureDictionaryData data = jss.Deserialize<TextureDictionaryData>(jsonText);

			AtlasContent atlasData = new AtlasContent();

			AtlasPage page = new AtlasPage();

			page.name = data.meta.image;
			page.format = (Format)Enum.Parse(typeof(Format), data.meta.format, false);

			// note: These values don't exist in this format, just pick the defaults
			page.minFilter = TextureFilter.Linear;
			page.magFilter = TextureFilter.Linear;
			page.uWrap = TextureWrap.ClampToEdge;
			page.vWrap = TextureWrap.ClampToEdge;

			string textureFolder = string.IsNullOrEmpty(TextureFolder) ? Path.GetDirectoryName(filename) : TextureFolder;
			string texturePath = Path.Combine(textureFolder, page.name);
			page.rendererObject = LoadTextureAsset(texturePath, out page.width, out page.height);

			atlasData.pages.Add(page);

			foreach (var frame in data.frames)
			{
				AtlasRegion region = new AtlasRegion();

				int x = frame.frame.x;
				int y = frame.frame.y;
				int w = frame.frame.w;
				int h = frame.frame.h;
				int oX = frame.spriteSourceSize.x;
				int oY = frame.spriteSourceSize.y;
				int oW = frame.sourceSize.w;
				int oH = frame.sourceSize.h;

				region.name = frame.filename;

				region.index = -1;
				region.page = page;

				region.rotate = frame.rotated;

				region.u = x / (float)region.page.width;
				region.v = y / (float)region.page.height;
				if (region.rotate)
				{
					region.u2 = (x + h) / (float)region.page.width;
					region.v2 = (y + w) / (float)region.page.height;

					float t = region.u;
					region.u = region.u2;
					region.u2 = t;

					t = region.v;
					region.v = region.v2;
					region.v2 = t;
				}
				else
				{
					region.u2 = (x + w) / (float)region.page.width;
					region.v2 = (y + h) / (float)region.page.height;
				}

				region.x = x;
				region.y = y;
				region.width = w;
				region.height = h;

				region.originalWidth = oW;
				region.originalHeight = oH;

				region.offsetX = oX;
				region.offsetY = oY;

				atlasData.regions.Add(region);
			}

			return atlasData;
		}

		AtlasContent LoadFromAtlas(string filename)
		{
			AtlasContent atlasData = new AtlasContent();

			using (TextReader reader = new StreamReader(filename))
			{
				string[] tuple = new string[4];
				AtlasPage page = null;
				while (true)
				{
					string line = reader.ReadLine();
					if (line == null)
						break;
					if (line.Trim().Length == 0)
						page = null;
					else if (page == null)
					{
						page = new AtlasPage();
						page.name = line;

						page.format = (Format)Enum.Parse(typeof(Format), readValue(reader), false);

						readTuple(reader, tuple);
						page.minFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[0]);
						page.magFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), tuple[1]);

						String direction = readValue(reader);
						page.uWrap = TextureWrap.ClampToEdge;
						page.vWrap = TextureWrap.ClampToEdge;
						if (direction == "x")
							page.uWrap = TextureWrap.Repeat;
						else if (direction == "y")
							page.vWrap = TextureWrap.Repeat;
						else if (direction == "xy")
							page.uWrap = page.vWrap = TextureWrap.Repeat;

						string textureFolder = string.IsNullOrEmpty(TextureFolder) ? Path.GetDirectoryName(filename) : TextureFolder;
						string texturePath = Path.Combine(textureFolder, line);
						page.rendererObject = LoadTextureAsset(texturePath, out page.width, out page.height);

						atlasData.pages.Add(page);
					}
					else
					{
						AtlasRegion region = new AtlasRegion();
						region.name = line;
						region.page = page;

						region.rotate = Boolean.Parse(readValue(reader));

						readTuple(reader, tuple);
						int x = int.Parse(tuple[0]);
						int y = int.Parse(tuple[1]);

						readTuple(reader, tuple);
						int width = int.Parse(tuple[0]);
						int height = int.Parse(tuple[1]);

						region.u = x / (float)page.width;
						region.v = y / (float)page.height;
						if (region.rotate)
						{
							region.u2 = (x + height) / (float)page.width;
							region.v2 = (y + width) / (float)page.height;
						}
						else
						{
							region.u2 = (x + width) / (float)page.width;
							region.v2 = (y + height) / (float)page.height;
						}
						region.x = x;
						region.y = y;
						region.width = Math.Abs(width);
						region.height = Math.Abs(height);

						if (readTuple(reader, tuple) == 4)
						{ // split is optional
							region.splits = new int[] {int.Parse(tuple[0]), int.Parse(tuple[1]),
								int.Parse(tuple[2]), int.Parse(tuple[3])};

							if (readTuple(reader, tuple) == 4)
							{ // pad is optional, but only present with splits
								region.pads = new int[] {int.Parse(tuple[0]), int.Parse(tuple[1]),
									int.Parse(tuple[2]), int.Parse(tuple[3])};

								readTuple(reader, tuple);
							}
						}

						region.originalWidth = int.Parse(tuple[0]);
						region.originalHeight = int.Parse(tuple[1]);

						readTuple(reader, tuple);
						region.offsetX = int.Parse(tuple[0]);
						region.offsetY = int.Parse(tuple[1]);

						region.index = int.Parse(readValue(reader));

						atlasData.regions.Add(region);
					}
				}
			}

			return atlasData;
		}

		ExternalReference<TextureContent> LoadTextureAsset(string filename, out int width, out int height)
		{
			using (Image img = Image.FromFile(filename))
			{
				width = (int)img.PhysicalDimension.Width;
				height = (int)img.PhysicalDimension.Height;
			}

			// todo: add TextureProcessor parameters
			return context.BuildAsset<TextureContent, TextureContent>(new ExternalReference<TextureContent>(filename), "TextureProcessor");
		}

		private ContentProcessorContext context;

		static String readValue(TextReader reader)
		{
			String line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1) throw new Exception("Invalid line: " + line);
			return line.Substring(colon + 1).Trim();
		}

		static int readTuple(TextReader reader, String[] tuple)
		{
			String line = reader.ReadLine();
			int colon = line.IndexOf(':');
			if (colon == -1) throw new Exception("Invalid line: " + line);
			int i = 0, lastMatch = colon + 1;
			for (; i < 3; i++)
			{
				int comma = line.IndexOf(',', lastMatch);
				if (comma == -1)
				{
					if (i == 0) throw new Exception("Invalid line: " + line);
					break;
				}
				tuple[i] = line.Substring(lastMatch, comma - lastMatch).Trim();
				lastMatch = comma + 1;
			}
			tuple[i] = line.Substring(lastMatch).Trim();
			return i + 1;
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

	[ContentTypeWriter]
	public class AtlasWriter : ContentTypeWriter<AtlasContent>
	{
		protected override void Write(ContentWriter output, AtlasContent value)
		{
			output.WriteObject(value.pages);
			output.WriteObject(value.regions);
		}

		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "ThirdPartyNinjas.NinjaSharp.Spine.AtlasReader, NinjaSharp";
		}
	}
}
