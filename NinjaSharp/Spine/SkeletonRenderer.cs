using Spine;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace ThirdPartyNinjas.NinjaSharp.Graphics
{
	public class SkeletonRenderer
	{
		public void Draw(SpriteBatchEx spriteBatch, Skeleton skeleton, Vector2 position, float rotation, Vector2 scale, Color tintColor, bool flipHorizontal, bool flipVertical)
		{
			List<Slot> drawOrder = skeleton.DrawOrder;
			float x = skeleton.X, y = skeleton.Y;
			float skeletonR = skeleton.R, skeletonG = skeleton.G, skeletonB = skeleton.B, skeletonA = skeleton.A;

			for (int i = 0, n = drawOrder.Count; i < n; i++)
			{
				Slot slot = drawOrder[i];

				RegionAttachment regionAttachment = slot.Attachment as RegionAttachment;
				if (regionAttachment != null)
				{
					AtlasRegion region = (AtlasRegion)regionAttachment.RendererObject;

					byte r = (byte)(skeletonR * slot.R * 255);
					byte g = (byte)(skeletonG * slot.G * 255);
					byte b = (byte)(skeletonB * slot.B * 255);
					byte a = (byte)(skeletonA * slot.A * 255);

					float offsetX = (regionAttachment.Offset[0] + regionAttachment.Offset[4]) / 2;
					float offsetY = (regionAttachment.Offset[1] + regionAttachment.Offset[5]) / 2;

					float m00 = slot.Bone.M00;
					float m01 = slot.Bone.M01;
					float m11 = slot.Bone.M11;
					float m10 = slot.Bone.M10;

					float localX = slot.Bone.WorldX + x;
					float localY = slot.Bone.WorldY + y;

					Texture2D texture = (Texture2D)region.page.rendererObject;

					// notes:
					// I'm not sure if multiplying the tint color against Spine's color is correct.
					// Keep in mind that we're usually in Premultipled Alpha mode here.
					spriteBatch.Draw(texture,
						new Vector2(offsetX * m00 + offsetY * m01 + localX, offsetX * m10 + offsetY * m11 + localY),
						new Rectangle(region.x, region.y, region.width, region.height),
						new Color(r * tintColor.R, g * tintColor.G, b * tintColor.B, a * tintColor.A),
						-(slot.Bone.WorldRotation + regionAttachment.Rotation) * 3.14159f / 180.0f,
						new Vector2(0.5f, 0.5f),
						new Vector2(slot.Bone.WorldScaleX, slot.Bone.WorldScaleY),
						region.rotate ? SpriteEffectsEx.RotatePackedUVs : SpriteEffectsEx.None,
						Matrix.CreateScale(flipHorizontal ? -scale.X : scale.X, flipVertical ? -scale.Y : scale.Y, 1) *
						Matrix.CreateRotationZ(rotation) *
						Matrix.CreateTranslation(position.X, position.Y, 0));
				}
			}
		}
	}
}
