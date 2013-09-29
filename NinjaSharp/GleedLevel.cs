using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace ThirdPartyNinjas.NinjaSharp
{
	public class GleedLevel
	{
		public class Item
		{
			public string Name;
			public Dictionary<string, string> CustomProperties;
			public bool Visible;
			public Vector2 Position;
		}

		public class TextureItem : Item
		{
			public bool FlipHorizontally;
			public bool FlipVertically;
			public Vector2 Origin;
			public float Rotation;
			public Vector2 Scale;
			public string TextureFileName;
			public Color TintColor;
		}

		public class PathItem : Item
		{
			public bool IsPolygon;
			public Color LineColor;
			public float LineWidth;
			public List<Vector2> LocalPoints;
			public List<Vector2> WorldPoints;
		}

		public class RectangleItem : Item
		{
			public Color FillColor;
			public float Width;
			public float Height;
		}

		public class CircleItem : Item
		{
			public Color FillColor;
			public float Radius;
		}

		public class Layer
		{
			public string Name;
			public bool Visible;
			public Vector2 ScrollSpeed;
			public List<Item> Items;
			public Dictionary<string, string> CustomProperties;
		}

		public string Name;
		public Dictionary<string, string> CustomProperties;
		public List<Layer> Layers;
	}
}
