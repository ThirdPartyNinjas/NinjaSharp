using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Spine;

using ThirdPartyNinjas.NinjaSharp.Graphics;

namespace ThirdPartyNinjas.NinjaSharp.Spine
{
	public class AnimationPlayer
	{
		public string CurrentAnimationName
		{
			get
			{
				if (animationStates.Count == 0)
					return "";
				return animationStates[animationStates.Count - 1].animation.Name;
			} 
		}

		public AnimationPlayer(SkeletonData skeletonData)
		{
			this.skeletonData = skeletonData;
			skeleton = new Skeleton(skeletonData);
			skeleton.SetSlotsToSetupPose();
			animationDataPool = new ObjectPool<AnimationData>(() => new AnimationData(), 10);
		}

		public void StartAnimation(string name, bool loop)
		{
			StartAnimation(name, loop, 0.0f);
		}

		public void StartAnimation(string name, bool loop, float startTime)
		{
			AnimationData animationData = animationDataPool.GetItem();
			animationData.animation = skeletonData.FindAnimation(name);
			animationData.currentApplyTime = startTime;
			animationData.previousApplyTime = startTime;
			animationData.transitionPosition = 1.0f;
			animationData.transitionDuration = 1.0f;
			animationData.loop = loop;

			animationStates.Clear();
			animationStates.Add(animationData);
		}

		public void TransitionAnimation(string name, bool loop, float mixTime)
		{
			TransitionAnimation(name, loop, mixTime, 0.0f);
		}

		public void TransitionAnimation(string name, bool loop, float mixTime, float startTime)
		{
			AnimationData animationData = new AnimationData();
			animationData.animation = skeletonData.FindAnimation(name);
			animationData.currentApplyTime = startTime;
			animationData.previousApplyTime = startTime;
			animationData.transitionPosition = 0;
			animationData.transitionDuration = mixTime;
			animationData.loop = loop;

			if (animationStates.Count > 0)
				animationStates[animationStates.Count - 1].previousApplyTime = animationStates[animationStates.Count - 1].currentApplyTime;

			animationStates.Add(animationData);
		}

		public bool Update(float deltaSeconds, List<Event> events)
		{
			bool animationCompleted = false;

			events.Clear();

			if (animationStates.Count == 0)
				return false;
        
			deltaSeconds *= timeScale;

			AnimationData currentAnimation = animationStates[animationStates.Count - 1];

			currentAnimation.currentApplyTime += deltaSeconds;
			if (currentAnimation.currentApplyTime > currentAnimation.animation.Duration)
			{
				if (currentAnimation.loop)
				{
					currentAnimation.currentApplyTime %= currentAnimation.animation.Duration;
				}
				else
				{
					currentAnimation.currentApplyTime = currentAnimation.animation.Duration;
					animationCompleted = true;
				}
			}

			if (animationStates.Count > 1)
			{
				currentAnimation.transitionPosition += deltaSeconds;
				if (currentAnimation.transitionPosition >= currentAnimation.transitionDuration)
				{
					currentAnimation.transitionPosition = currentAnimation.transitionDuration;
					for (int i = 0; i < animationStates.Count - 1; i++)
						animationDataPool.ReturnItem(animationStates[i]);
					animationStates.RemoveRange(0, animationStates.Count - 1);
				}
			}

			bool first = false;
			foreach (var animationData in animationStates)
			{
				if (first)
					animationData.animation.Apply(skeleton, animationData.previousApplyTime, animationData.currentApplyTime, animationData.loop, events);
				else
					animationData.animation.Mix(skeleton, animationData.previousApplyTime, animationData.currentApplyTime, animationData.loop, events, animationData.transitionPosition / animationData.transitionDuration);
				first = false;
			}

			skeleton.UpdateWorldTransform();
			return animationCompleted;
		}

		public void Draw(SpriteBatchEx spriteBatch, SkeletonRenderer skeletonRenderer, Vector2 position, float rotation, Vector2 scale, Color tintColor, bool flipHorizontal, bool flipVertical)
		{
			skeletonRenderer.Draw(spriteBatch, skeleton, position, rotation, scale, tintColor, flipHorizontal, flipVertical);
		}

		Skeleton skeleton;
		SkeletonData skeletonData;

		float timeScale = 1.0f;

		ObjectPool<AnimationData> animationDataPool;
		List<AnimationData> animationStates = new List<AnimationData>();

		class AnimationData
		{
			public Animation animation;
			public float currentApplyTime, previousApplyTime;
			public float transitionPosition, transitionDuration;
			public bool loop;
		}
	}
}
