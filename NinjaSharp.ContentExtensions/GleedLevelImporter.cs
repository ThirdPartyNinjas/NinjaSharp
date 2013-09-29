using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace ThirdPartyNinjas.NinjaSharp.ContentExtensions
{
	[ContentImporter(".gleed", DisplayName = "Gleed Level Importer - ThirdPartyNinjas")]
	public class GleedLevelImporter : ContentImporter<GleedLevel>
	{
		public override GleedLevel Import(string fileName, ContentImporterContext context)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);

			GleedLevel level = new GleedLevel();
			level.CustomProperties = new Dictionary<string, string>();
			level.Layers = new List<GleedLevel.Layer>();
			level.Name = xmlDocument.DocumentElement.Attributes["Name"].Value;

			foreach (XmlElement propertyNode in xmlDocument.SelectNodes("Level/CustomProperties/Property"))
			{
				if (propertyNode.Attributes["Type"].Value != "string")
					continue;
				level.CustomProperties[propertyNode.Attributes["Name"].Value] = propertyNode["string"].InnerText;
			}

			foreach (XmlElement layerNode in xmlDocument.SelectNodes("Level/Layers/Layer"))
			{
				GleedLevel.Layer layer = new GleedLevel.Layer();

				layer.Name = layerNode.Attributes["Name"].Value;
				layer.Visible = layerNode.Attributes["Visible"].Value == "true";
				layer.ScrollSpeed.X = float.Parse(layerNode["ScrollSpeed"]["X"].InnerText, CultureInfo.InvariantCulture);
				layer.ScrollSpeed.Y = float.Parse(layerNode["ScrollSpeed"]["Y"].InnerText, CultureInfo.InvariantCulture);

				layer.Items = new List<GleedLevel.Item>();
				foreach (XmlElement itemNode in layerNode.SelectNodes("Items/Item"))
				{
					GleedLevel.Item item;

					switch (itemNode.Attributes["xsi:type"].Value)
					{
						case "TextureItem":
							GleedLevel.TextureItem ti = new GleedLevel.TextureItem();
							ti.FlipHorizontally = itemNode["FlipHorizontally"].InnerText == "true";
							ti.FlipVertically = itemNode["FlipVertically"].InnerText == "true";
							ti.Origin.X = float.Parse(itemNode["Origin"]["X"].InnerText, CultureInfo.InvariantCulture);
							ti.Origin.Y = float.Parse(itemNode["Origin"]["Y"].InnerText, CultureInfo.InvariantCulture);
							ti.Rotation = float.Parse(itemNode["Rotation"].InnerText, CultureInfo.InvariantCulture);
							ti.Scale.X = float.Parse(itemNode["Scale"]["X"].InnerText, CultureInfo.InvariantCulture);
							ti.Scale.Y = float.Parse(itemNode["Scale"]["Y"].InnerText, CultureInfo.InvariantCulture);
							ti.TextureFileName = itemNode["texture_filename"].InnerText;
							ti.TintColor.R = byte.Parse(itemNode["TintColor"]["R"].InnerText, CultureInfo.InvariantCulture);
							ti.TintColor.G = byte.Parse(itemNode["TintColor"]["G"].InnerText, CultureInfo.InvariantCulture);
							ti.TintColor.B = byte.Parse(itemNode["TintColor"]["B"].InnerText, CultureInfo.InvariantCulture);
							ti.TintColor.A = byte.Parse(itemNode["TintColor"]["A"].InnerText, CultureInfo.InvariantCulture);
							item = ti;
							break;

						case "PathItem":
							GleedLevel.PathItem pi = new GleedLevel.PathItem();
							pi.IsPolygon = itemNode["IsPolygon"].InnerText == "true";
							pi.LineColor.R = byte.Parse(itemNode["LineColor"]["R"].InnerText, CultureInfo.InvariantCulture);
							pi.LineColor.G = byte.Parse(itemNode["LineColor"]["G"].InnerText, CultureInfo.InvariantCulture);
							pi.LineColor.B = byte.Parse(itemNode["LineColor"]["B"].InnerText, CultureInfo.InvariantCulture);
							pi.LineColor.A = byte.Parse(itemNode["LineColor"]["A"].InnerText, CultureInfo.InvariantCulture);
							pi.LineWidth = float.Parse(itemNode["LineWidth"].InnerText, CultureInfo.InvariantCulture);
							pi.LocalPoints = new List<Vector2>();
							foreach (XmlElement localPointsNode in itemNode.SelectNodes("LocalPoints/Vector2"))
							{
								Vector2 v;
								v.X = float.Parse(localPointsNode["X"].InnerText, CultureInfo.InvariantCulture);
								v.Y = float.Parse(localPointsNode["Y"].InnerText, CultureInfo.InvariantCulture);
								pi.LocalPoints.Add(v);
							}
							pi.WorldPoints = new List<Vector2>();
							foreach (XmlElement worldPointsNode in itemNode.SelectNodes("WorldPoints/Vector2"))
							{
								Vector2 v;
								v.X = float.Parse(worldPointsNode["X"].InnerText, CultureInfo.InvariantCulture);
								v.Y = float.Parse(worldPointsNode["Y"].InnerText, CultureInfo.InvariantCulture);
								pi.WorldPoints.Add(v);
							}
							item = pi;
							break;

						case "RectangleItem":
							GleedLevel.RectangleItem ri = new GleedLevel.RectangleItem();
							ri.FillColor.R = byte.Parse(itemNode["FillColor"]["R"].InnerText, CultureInfo.InvariantCulture);
							ri.FillColor.G = byte.Parse(itemNode["FillColor"]["G"].InnerText, CultureInfo.InvariantCulture);
							ri.FillColor.B = byte.Parse(itemNode["FillColor"]["B"].InnerText, CultureInfo.InvariantCulture);
							ri.FillColor.A = byte.Parse(itemNode["FillColor"]["A"].InnerText, CultureInfo.InvariantCulture);
							ri.Width = float.Parse(itemNode["Width"].InnerText, CultureInfo.InvariantCulture);
							ri.Height = float.Parse(itemNode["Height"].InnerText, CultureInfo.InvariantCulture);
							item = ri;
							break;

						case "CircleItem":
							GleedLevel.CircleItem ci = new GleedLevel.CircleItem();
							ci.FillColor.R = byte.Parse(itemNode["FillColor"]["R"].InnerText, CultureInfo.InvariantCulture);
							ci.FillColor.G = byte.Parse(itemNode["FillColor"]["G"].InnerText, CultureInfo.InvariantCulture);
							ci.FillColor.B = byte.Parse(itemNode["FillColor"]["B"].InnerText, CultureInfo.InvariantCulture);
							ci.FillColor.A = byte.Parse(itemNode["FillColor"]["A"].InnerText, CultureInfo.InvariantCulture);
							ci.Radius = float.Parse(itemNode["Radius"].InnerText, CultureInfo.InvariantCulture);
							item = ci;
							break;
						default:
							throw new Exception("Unknown Gleed item type encountered");
					}

					item.Name = itemNode.Attributes["Name"].Value;
					item.Visible = itemNode.Attributes["Visible"].Value == "true";
					item.Position.X = float.Parse(itemNode["Position"]["X"].InnerText, CultureInfo.InvariantCulture);
					item.Position.Y = float.Parse(itemNode["Position"]["Y"].InnerText, CultureInfo.InvariantCulture);
					item.CustomProperties = new Dictionary<string, string>();
					foreach (XmlElement propertyNode in itemNode.SelectNodes("CustomProperties/Property"))
					{
						if (propertyNode.Attributes["Type"].Value != "string")
							continue;
						item.CustomProperties[propertyNode.Attributes["Name"].Value] = propertyNode["string"].InnerText;
					}

					layer.Items.Add(item);
				}

				layer.CustomProperties = new Dictionary<string, string>();
				foreach (XmlElement propertyNode in layerNode.SelectNodes("CustomProperties/Property"))
				{
					if (propertyNode.Attributes["Type"].Value != "string")
						continue;
					layer.CustomProperties[propertyNode.Attributes["Name"].Value] = propertyNode["string"].InnerText;
				}

				level.Layers.Add(layer);
			}

			return level;
		}
	}
}
