using System;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Text;

namespace ThirdPartyNinjas.NinjaSharp.Graphics
{
	// Notes:
	// This class aims to match XNAs SpriteBatch, with a few changes.
	// 1) Only deferred draw mode is supported.
	// 2) Origins are specified in the range of 0-1, rather than 0-Width
	// 3) The default cull mode is CullNone, but other modes should work just fine
	// 4) A new draw function that takes in a transform matrix

	// todo: Add a method to allow direct vertex access

	public class SpriteBatchEx
	{
		public SpriteBatchEx(GraphicsDevice graphicsDevice, Effect effect)
			: this(graphicsDevice, effect, 1000)
		{
			// note: Don't do anything here. Add to SpriteBatch(GraphicsDevice, int) instead
		}

		public SpriteBatchEx(GraphicsDevice graphicsDevice, Effect defaultEffect, int maxSprites)
		{
			if (graphicsDevice == null)
				throw new Exception("GraphicsDevice cannot be null");

			this.graphicsDevice = graphicsDevice;

			this.defaultEffect = defaultEffect;

			ResizeBuffers(maxSprites);

			whitePixel = new Texture2D(graphicsDevice, 1, 1);
			whitePixel.SetData<Color>(new Color[] { Color.White });
		}

		public void Begin()
		{
			this.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.Identity);
		}

		public void Begin(SpriteSortMode sortMode, BlendState blendState)
		{
			this.Begin(sortMode, blendState, null, null, null, null, Matrix.Identity);
		}

		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
		{
			this.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, Matrix.Identity);
		}

		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
		{
			this.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.Identity);
		}

		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
		{
			if (insideBeginEnd)
				throw new Exception("End must be called before calling Begin again.");

			if (sortMode != SpriteSortMode.Deferred)
				throw new Exception("Only SpriteSortMode.Deferred is currently supported.");

			this.sortMode = sortMode;
			this.blendState = blendState ?? BlendState.AlphaBlend;
			this.samplerState = samplerState ?? SamplerState.LinearClamp;
			this.depthStencilState = depthStencilState ?? DepthStencilState.None;
			this.rasterizerState = rasterizerState ?? RasterizerState.CullNone;
			customEffect = effect;
			batchTransformMatrix = transformMatrix;

			insideBeginEnd = true;
		}

		public void End()
		{
			if (!insideBeginEnd)
				throw new Exception("Begin must be called before calling End.");

			if (activeVertices >= 4)
				DrawBuffer();

			insideBeginEnd = false;
		}

		public void Draw(Texture2D texture, Vector2 position, Color color)
		{
			Draw(texture, position, null, color, 0, Vector2.Zero, Vector2.One, SpriteEffectsEx.None, 0);
		}

		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			Draw(texture, position, sourceRectangle, color, 0, Vector2.Zero, Vector2.One, SpriteEffectsEx.None, 0);
		}

		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffectsEx effects, float layerDepth)
		{
			Draw(texture, position, sourceRectangle, color, rotation, origin, new Vector2(scale), effects, layerDepth);
		}

		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffectsEx effects, float layerDepth)
		{
			Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, Matrix.Identity);
		}

		public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
		{
			Draw(texture, destinationRectangle, null, color, 0, Vector2.Zero, SpriteEffectsEx.None, 0);
		}

		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			Draw(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffectsEx.None, 0);
		}

		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffectsEx effects, float layerDepth)
		{
			float width = (sourceRectangle.HasValue) ? sourceRectangle.Value.Width : texture.Width;
			float height = (sourceRectangle.HasValue) ? sourceRectangle.Value.Height : texture.Height;

			Draw(texture, new Vector2(destinationRectangle.X, destinationRectangle.Y), sourceRectangle, color, rotation, origin, new Vector2(destinationRectangle.Width / width, destinationRectangle.Height / height), effects, Matrix.Identity);
		}

		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffectsEx effects, Matrix transformMatrix)
		{
			if (activeVertices == maxVertices || (currentTexture != null && !currentTexture.Equals(texture)))
				DrawBuffer();

			currentTexture = texture;

			Rectangle textureRect = new Rectangle(0, 0, texture.Width, texture.Height);
			Rectangle rectangle;

			if (sourceRectangle.HasValue)
				rectangle = sourceRectangle.Value;
			else
				rectangle = textureRect;

			int width = rectangle.Width;
			int height = rectangle.Height;

			int textureWidth = texture.Width;
			int textureHeight = texture.Height;

			Vector2 uvTL, uvTR, uvBL, uvBR;
			Vector2 temp;

			if ((effects & SpriteEffectsEx.RotatePackedUVs) != SpriteEffectsEx.None)
			{
				uvTL = new Vector2((float)(rectangle.X + rectangle.Height) / textureWidth, (float)rectangle.Y / textureHeight);
				uvTR = new Vector2((float)(rectangle.X + rectangle.Height) / textureWidth, (float)(rectangle.Y + rectangle.Width) / textureHeight);
				uvBL = new Vector2((float)rectangle.X / textureWidth, (float)rectangle.Y / textureHeight);
				uvBR = new Vector2((float)rectangle.X / textureWidth, (float)(rectangle.Y + rectangle.Width) / textureHeight);
			}
			else
			{
				uvTL = new Vector2(rectangle.X / (float)textureWidth, rectangle.Y / (float)textureHeight);
				uvTR = new Vector2((rectangle.X + rectangle.Width) / (float)textureWidth, rectangle.Y / (float)textureHeight);
				uvBL = new Vector2(rectangle.X / (float)textureWidth, (rectangle.Y + rectangle.Height) / (float)textureHeight);
				uvBR = new Vector2((rectangle.X + rectangle.Width) / (float)textureWidth, (rectangle.Y + rectangle.Height) / (float)textureHeight);
			}

			if ((effects & SpriteEffectsEx.FlipVertically) != SpriteEffectsEx.None)
			{
				temp = uvTL;
				uvTL = uvBL;
				uvBL = temp;
				temp = uvTR;
				uvTR = uvBR;
				uvBR = temp;
			}
			if ((effects & SpriteEffectsEx.FlipHorizontally) != SpriteEffectsEx.None)
			{
				temp = uvTL;
				uvTL = uvTR;
				uvTR = temp;
				temp = uvBL;
				uvBL = uvBR;
				uvBR = temp;
			}

			Vector3 v;
			Matrix transform = Matrix.CreateScale(scale.X, scale.Y, 1.0f) *
				Matrix.CreateRotationZ(rotation) *
				Matrix.CreateTranslation(position.X, position.Y, 0) *
				transformMatrix;

			v.X = -width * origin.X;
			v.Y = -height * origin.Y;
			v.Z = 0;
			v = Vector3.Transform(v, transform);

			vertices[activeVertices].Position = new Microsoft.Xna.Framework.Vector3(v.X, v.Y, 0);
			vertices[activeVertices].Color = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
			vertices[activeVertices++].TextureCoordinate = new Microsoft.Xna.Framework.Vector2(uvTL.X, uvTL.Y);

			v.X = width * (1 - origin.X);
			v.Y = -height * origin.Y;
			v.Z = 0;
			v = Vector3.Transform(v, transform);

			vertices[activeVertices].Position = new Microsoft.Xna.Framework.Vector3(v.X, v.Y, 0);
			vertices[activeVertices].Color = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
			vertices[activeVertices++].TextureCoordinate = new Microsoft.Xna.Framework.Vector2(uvTR.X, uvTR.Y);

			v.X = -width * origin.X;
			v.Y = height * (1 - origin.Y);
			v.Z = 0;
			v = Vector3.Transform(v, transform);

			vertices[activeVertices].Position = new Microsoft.Xna.Framework.Vector3(v.X, v.Y, 0);
			vertices[activeVertices].Color = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
			vertices[activeVertices++].TextureCoordinate = new Microsoft.Xna.Framework.Vector2(uvBL.X, uvBL.Y);

			v.X = width * (1 - origin.X);
			v.Y = height * (1 - origin.Y);
			v.Z = 0;
			v = Vector3.Transform(v, transform);

			vertices[activeVertices].Position = new Microsoft.Xna.Framework.Vector3(v.X, v.Y, 0);
			vertices[activeVertices].Color = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
			vertices[activeVertices++].TextureCoordinate = new Microsoft.Xna.Framework.Vector2(uvBR.X, uvBR.Y);
		}

		public void DrawString(SpriteFontEx spriteFont, string text, Vector2 position, Color color)
		{
			DrawString(spriteFont, text, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffectsEx.None, Matrix.Identity);
		}

		public void DrawString(SpriteFontEx spriteFont, StringBuilder text, Vector2 position, Color color)
		{
			DrawString(spriteFont, text.ToString(), position, color, 0, Vector2.Zero, Vector2.One, SpriteEffectsEx.None, Matrix.Identity);
		}

		public void DrawString(SpriteFontEx spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffectsEx effects, float layerDepth)
		{
			DrawString(spriteFont, text, position, color, rotation, origin, new Vector2(scale), effects, Matrix.Identity);
		}

		public void DrawString(SpriteFontEx spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffectsEx effects, float layerDepth)
		{
			DrawString(spriteFont, text.ToString(), position, color, rotation, origin, new Vector2(scale), effects, Matrix.Identity);
		}

		public void DrawString(SpriteFontEx spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffectsEx effects, float layerDepth)
		{
			DrawString(spriteFont, text, position, color, rotation, origin, scale, effects, Matrix.Identity);
		}

		public void DrawString(SpriteFontEx spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffectsEx effects, float layerDepth)
		{
			DrawString(spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, Matrix.Identity);
		}

		public void DrawString(SpriteFontEx spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffectsEx effects, Matrix transformMatrix)
		{
			if (activeVertices == maxVertices || (currentTexture != null && !currentTexture.Equals(spriteFont.Texture)))
				DrawBuffer();

			Vector2 stringSize = spriteFont.MeasureString(text);

			origin.X *= stringSize.X;
			origin.Y *= stringSize.Y;

			if ((effects & SpriteEffectsEx.FlipHorizontally) != SpriteEffectsEx.None)
			{
				scale.X *= -1;
				position.X += stringSize.X;
			}
			if ((effects & SpriteEffectsEx.FlipVertically) != SpriteEffectsEx.None)
			{
				scale.Y *= -1;
				position.Y += stringSize.Y;
			}

			currentTexture = spriteFont.Texture;

			float x = 0, y = 0;

			foreach (char c in text)
			{
				if (c == '\n')
				{
					y += spriteFont.LineSpacing;
					continue;
				}
				if (c == '\r')
				{
					x = 0;
					continue;
				}

				SpriteFontEx.CharacterData cd = spriteFont[c];

				if (cd.width == 0 || cd.height == 0)
				{
					x += cd.advanceX;
					continue;
				}

				if (activeVertices == maxVertices)
					DrawBuffer();

				int width = cd.width;
				int height = cd.height;

				int textureWidth = currentTexture.Width;
				int textureHeight = currentTexture.Height;

				Vector3 v;
				Matrix transform = Matrix.CreateTranslation(x + cd.offsetX, y + cd.offsetY, 0) *
					Matrix.CreateTranslation(-origin.X, -origin.Y, 0) *
					Matrix.CreateScale(scale.X, scale.Y, 1.0f) *
					Matrix.CreateRotationZ(rotation) *
					Matrix.CreateTranslation(position.X, position.Y, 0) *
					transformMatrix;

				v = new Vector3(0, 0, 0);
				v = Vector3.Transform(v, transform);

				vertices[activeVertices].Position = v;
				vertices[activeVertices].Color = color;
				vertices[activeVertices++].TextureCoordinate = new Vector2((float)cd.x / textureWidth, (float)cd.y / textureHeight);

				v = new Vector3(width, 0, 0);
				v = Vector3.Transform(v, transform);

				vertices[activeVertices].Position = v;
				vertices[activeVertices].Color = color;
				vertices[activeVertices++].TextureCoordinate = new Vector2((float)(cd.x + cd.width) / textureWidth, (float)cd.y / textureHeight);

				v = new Vector3(0, height, 0);
				v = Vector3.Transform(v, transform);

				vertices[activeVertices].Position = v;
				vertices[activeVertices].Color = color;
				vertices[activeVertices++].TextureCoordinate = new Vector2((float)cd.x / textureWidth, (float)(cd.y + cd.height) / textureHeight);

				v = new Vector3(width, height, 0);
				v = Vector3.Transform(v, transform);

				vertices[activeVertices].Position = v;
				vertices[activeVertices].Color = color;
				vertices[activeVertices++].TextureCoordinate = new Vector2((float)(cd.x + cd.width) / textureWidth, (float)(cd.y + cd.height) / textureHeight);

				x += cd.advanceX;
			}
		}

		public void DrawString(SpriteFontEx spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffectsEx effects, Matrix transformMatrix)
		{
			DrawString(spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, transformMatrix);
		}

		public void DrawLine(Vector2 start, Vector2 end, float thickness, Color color)
		{
			Vector2 diff = end - start;
			float angle = (float)Math.Atan2(diff.Y, diff.X);

			Draw(whitePixel, (end + start) / 2, null, color, angle, new Vector2(0.5f), new Vector2(diff.Length(), thickness), SpriteEffectsEx.None, 0);
		}

		public void DrawRectangle(Rectangle rectangle, Color color)
		{
			Draw(whitePixel, rectangle, null, color);
		}

		public void DrawRectangle(Rectangle rectangle, Color color, float rotation, Vector2 origin)
		{
			Draw(whitePixel, rectangle, null, color, rotation, origin, SpriteEffectsEx.None, 0);
		}

		void DrawBuffer()
		{
			graphicsDevice.BlendState = blendState;
			graphicsDevice.SamplerStates[0] = samplerState;
			graphicsDevice.DepthStencilState = depthStencilState;
			graphicsDevice.RasterizerState = rasterizerState;

			Viewport viewport = graphicsDevice.Viewport;
			defaultEffect.Parameters["Viewport"].SetValue(new Vector2(viewport.Width, viewport.Height));
			defaultEffect.Parameters["TransformMatrix"].SetValue(batchTransformMatrix);
			defaultEffect.CurrentTechnique.Passes[0].Apply();

			if (customEffect != null)
			{
				foreach (EffectPass pass in customEffect.CurrentTechnique.Passes)
				{
					pass.Apply();
					graphicsDevice.Textures[0] = currentTexture;
					graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, vertices, 0, activeVertices, indices, 0, (activeVertices / 4) * 2);
				}
			}

			graphicsDevice.Textures[0] = currentTexture;
			graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, vertices, 0, activeVertices, indices, 0, (activeVertices / 4) * 2);

			activeVertices = 0;
			currentTexture = null;
		}

		protected void ResizeBuffers(int maxSprites)
		{
			maxVertices = maxSprites * 4;

			Array.Resize<VertexPositionColorTexture>(ref vertices, maxSprites * 4);
			Array.Resize<short>(ref indices, maxSprites * 6);

			for (int i = 0; i < maxSprites; i++)
			{
				indices[i * 6 + 0] = (short)(i * 4);
				indices[i * 6 + 1] = (short)(i * 4 + 1);
				indices[i * 6 + 2] = (short)(i * 4 + 2);
				indices[i * 6 + 3] = (short)(i * 4 + 2);
				indices[i * 6 + 4] = (short)(i * 4 + 1);
				indices[i * 6 + 5] = (short)(i * 4 + 3);
			}
		}

		bool insideBeginEnd = false;
		GraphicsDevice graphicsDevice;
		Effect defaultEffect;
		Texture2D currentTexture;
		Matrix batchTransformMatrix;
		Texture2D whitePixel;

		VertexPositionColorTexture[] vertices;
		short[] indices;

		int maxVertices;
		int activeVertices = 0;

		SpriteSortMode sortMode;
		BlendState blendState;
		SamplerState samplerState;
		DepthStencilState depthStencilState;
		RasterizerState rasterizerState;
		Effect customEffect;
	}
}
