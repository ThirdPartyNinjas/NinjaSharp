using System;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ThirdPartyNinjas.NinjaSharp.Graphics
{
	// Notes:
	// This class aims to match XNAs SpriteBatch, with a few changes.
	// 1) Only deferred draw mode is supported.
	// 2) Origins are specified in the range of 0-1, rather than 0-Width
	// 3) The default cull mode is CullNone, but other modes should work just fine
	// 4) A new draw function that takes in a transform matrix
	// 5) DrawString isn't yet implemented

	// todo: Add a method to allow direct vertex access
	// todo: DrawString functions

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

			Draw(texture, new Vector2(destinationRectangle.X, destinationRectangle.Y), sourceRectangle, color, rotation, origin, new Vector2(width / destinationRectangle.Width, height / destinationRectangle.Height), effects, Matrix.Identity);
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
		Texture currentTexture;
		Matrix batchTransformMatrix;

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
