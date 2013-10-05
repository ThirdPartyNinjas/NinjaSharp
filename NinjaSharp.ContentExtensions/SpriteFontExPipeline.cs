using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using ThirdPartyNinjas.NinjaSharp.Graphics;
using System.ComponentModel;
using System.IO;

namespace ThirdPartyNinjas.NinjaSharp.ContentExtensions
{
	[ContentSerializerRuntimeType("ThirdPartyNinjas.NinjaSharp.Graphics.SpriteFontEx, NinjaSharp")]
	public class SpriteFontExContent
	{
		public int LineSpacing { get; set; }
		public Dictionary<char, SpriteFontEx.CharacterData> characterData = new Dictionary<char, SpriteFontEx.CharacterData>();
		public ExternalReference<TextureContent> texture;
	}

	[ContentImporter(".fnt", DisplayName = "SpriteFontEx Importer - ThirdPartyNinjas", DefaultProcessor = "SpriteFontExProcessor")]
	public class SpriteFontExImporter : ContentImporter<string>
	{
		public override string Import(string filename, ContentImporterContext context)
		{
			return filename;
		}
	}

	[ContentProcessor(DisplayName = "SpriteFontEx Processor - ThirdPartyNinjas")]
	public class SpriteFontExProcessor : ContentProcessor<string, SpriteFontExContent>
	{
		[DisplayName("Texture Folder")]
		[Description("The folder to look for the sprite texture.")]
		public string TextureFolder { get; set; }

		public override SpriteFontExContent Process(string filename, ContentProcessorContext context)
		{
			SpriteFontExContent data = new SpriteFontExContent();

			XDocument xDocument = XDocument.Load(filename);

			XElement commonElement = xDocument.Element("font").Element("common");
			data.LineSpacing = int.Parse(commonElement.Attribute("lineHeight").Value, CultureInfo.InvariantCulture);

			foreach(XElement element in xDocument.Element("font").Element("chars").Elements("char"))
			{
				SpriteFontEx.CharacterData characterData = new SpriteFontEx.CharacterData();

				characterData.x = int.Parse(element.Attribute("x").Value, CultureInfo.InvariantCulture);
				characterData.y = int.Parse(element.Attribute("y").Value, CultureInfo.InvariantCulture);
				characterData.width = int.Parse(element.Attribute("width").Value, CultureInfo.InvariantCulture);
				characterData.height = int.Parse(element.Attribute("height").Value, CultureInfo.InvariantCulture);
				characterData.offsetX = int.Parse(element.Attribute("xoffset").Value, CultureInfo.InvariantCulture);
				characterData.offsetY = int.Parse(element.Attribute("yoffset").Value, CultureInfo.InvariantCulture);
				characterData.advanceX = int.Parse(element.Attribute("xadvance").Value, CultureInfo.InvariantCulture);

				int id = int.Parse(element.Attribute("id").Value, CultureInfo.InvariantCulture);
				data.characterData.Add((char)id, characterData);
			}

			string textureFilename = xDocument.Element("font").Element("pages").Element("page").Attribute("file").Value;
			string textureFolder = string.IsNullOrEmpty(TextureFolder) ? Path.GetDirectoryName(filename) : TextureFolder;
			string texturePath = Path.Combine(textureFolder, textureFilename);

			data.texture = context.BuildAsset<TextureContent, TextureContent>(new ExternalReference<TextureContent>(textureFilename), "TextureProcessor");
			return data;
		}
	}
}
