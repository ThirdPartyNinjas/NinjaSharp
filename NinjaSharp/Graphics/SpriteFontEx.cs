using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ThirdPartyNinjas.NinjaSharp.Graphics
{
	// Note: Differences from XNA SpriteFont
	// 1) Doesn't support kerning (yet?)

	public class SpriteFontEx
	{
		public class CharacterData
		{
			public int x, y;
			public int width, height;
			public int offsetX, offsetY;
			public int advanceX;
		}

		[ContentSerializer]
		public int LineSpacing { get; set; }

		[ContentSerializerIgnore]
		public float Spacing { get; set; }

		[ContentSerializerIgnore]
		public char? DefaultCharacter
		{
			get
			{
				return defaultCharacter;
			}
			set
			{
				if(!value.HasValue || !characterData.ContainsKey(value.Value))
				{
					throw new Exception("Font doesn't contain the requested DefaultCharacter: " + value.Value);
				}
				defaultCharacter = value;
			}
		}

		[ContentSerializerIgnore]
		public ReadOnlyCollection<char> Characters
		{
			get
			{
				if (characters == null)
					characters = new ReadOnlyCollection<char>(new List<char>(characterData.Keys));

				return characters;
			}
		}

		[ContentSerializerIgnore]
		public CharacterData this[char c]
		{
			get
			{
				CharacterData value;
				if (characterData.TryGetValue(c, out value))
					return value;
				if (!DefaultCharacter.HasValue)
					throw new Exception("Font doesn't contain the requested character: " + c);
				return characterData[DefaultCharacter.Value];
			}
		}

		[ContentSerializerIgnore]
		public Texture2D Texture { get { return texture as Texture2D; } }

		public SpriteFontEx()
		{
			Spacing = 0;
		}

		public Vector2 MeasureString(string text)
		{
			float x = 0;
			Vector2 measurement = Vector2.Zero;

			foreach (char c in text)
			{
				if (c == '\n')
				{
					measurement.Y += LineSpacing;
				}
				else if (c == '\r')
				{
					measurement.X = Math.Max(measurement.X, x);
					x = 0;
				}
				else
				{
					CharacterData cd = this[c];
					x += cd.advanceX;
				}
			}

			measurement.X = Math.Max(measurement.X, x);
			measurement.Y += LineSpacing;

			return measurement;
		}

		public Vector2 MeasureString(StringBuilder stringBuilder)
		{
			return MeasureString(stringBuilder.ToString());
		}

		[ContentSerializerIgnore]
		protected char? defaultCharacter;
		[ContentSerializerIgnore]
		protected ReadOnlyCollection<char> characters;

		[ContentSerializer]
		protected Dictionary<char, CharacterData> characterData;
		[ContentSerializer]
		protected Texture texture;
	}
}
