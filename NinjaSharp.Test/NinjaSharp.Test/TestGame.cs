using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ThirdPartyNinjas.NinjaSharp;
using ThirdPartyNinjas.NinjaSharp.Graphics;
using ThirdPartyNinjas.NinjaSharp.Spine;

using Spine;

namespace NinjaSharp.Test
{
	public class TestGame : Game
	{
		public TestGame()
		{
			new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1280,
				PreferredBackBufferHeight = 720,
			};

			IsMouseVisible = true;
			IsFixedTimeStep = false;
			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			Effect spriteBatchEffect = Content.Load<Effect>("SpriteBatchEffect");
			spriteBatch = new SpriteBatchEx(GraphicsDevice, spriteBatchEffect);

			Bone.yDown = true;
			skeletonData = Content.Load<SkeletonData>("spineboy/spineboy");
			skeleton = new Skeleton(skeletonData);
			skeleton.SetSlotsToSetupPose();
			
			AnimationStateData stateData = new AnimationStateData(skeleton.Data);
			animationState = new AnimationState(stateData);
			animationState.SetAnimation(0, "walk", true);

			skeleton.UpdateWorldTransform();
		}

		protected override void UnloadContent()
		{
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			animationState.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);
			animationState.Apply(skeleton);
			skeleton.UpdateWorldTransform();
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin();
			skeletonRenderer.Draw(spriteBatch, skeleton, new Vector2(640, 500), 0, Vector2.One, Color.White, false, false);
			spriteBatch.End();

			base.Draw(gameTime);
		}

#if WINDOWS || XBOX
		static void Main(string[] args)
		{
			using (TestGame game = new TestGame())
			{
				game.Run();
			}
		}
#endif

		Skeleton skeleton;
		SpriteBatchEx spriteBatch;
		SkeletonRenderer skeletonRenderer = new SkeletonRenderer();
		SkeletonData skeletonData;
		AnimationState animationState;
	}
}
