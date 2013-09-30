using System.Collections.Generic;

namespace ThirdPartyNinjas.NinjaSharp
{
	public class ScreenManager
	{
		public void Update(float deltaSeconds)
		{
			screens.AddRange(screensToAdd);
			screensToAdd.Clear();

			GameScreen waitingScreen = null;
			bool hasFocus = true;
			bool foundExclusive = false;

			for (int i = screens.Count - 1; i >= 0; i--)
			{
				GameScreen gameScreen = screens[i];

				if (gameScreen.ScreenState == GameScreen.State.Waiting)
				{
					if (waitingScreen == null)
						waitingScreen = gameScreen;
					else
						gameScreen.Exit(false);
				}
				else
				{
					if (gameScreen.IsExclusive)
					{
						if (foundExclusive)
							gameScreen.Exit(true);
						else
							foundExclusive = true;
					}

					gameScreen.Update(deltaSeconds, hasFocus);
					hasFocus = false;
				}
			}

			foundExclusive = false;
			waitingScreen = null;

			int index = 0;
			while (index < screens.Count)
			{
				if (screens[index].Remove)
				{
					screens.RemoveAt(index);
				}
				else
				{
					if (screens[index].ScreenState == GameScreen.State.Waiting)
						waitingScreen = screens[index];
					else
						foundExclusive = true;
					index++;
				}
			}

			if (waitingScreen != null && !foundExclusive)
			{
				waitingScreen.Update(0, true);
			}
		}

		public void Draw()
		{
			for (int i = screens.Count - 1; i >= 0; i--)
			{
				if (screens[i].ScreenState != GameScreen.State.Waiting)
					screens[i].Draw();
			}
		}

		public void AddScreen(GameScreen gameScreen)
		{
			gameScreen.ScreenManager = this;

			if (!gameScreen.IsExclusive)
				gameScreen.ScreenState = GameScreen.State.TransitionOn;
			else
				gameScreen.ScreenState = GameScreen.State.Waiting;

			screensToAdd.Add(gameScreen);
		}

		List<GameScreen> screens = new List<GameScreen>();
		List<GameScreen> screensToAdd = new List<GameScreen>();
	}
}
