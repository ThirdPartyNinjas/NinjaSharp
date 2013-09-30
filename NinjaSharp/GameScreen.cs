namespace ThirdPartyNinjas.NinjaSharp
{
	public abstract class GameScreen
	{
		public enum State
		{
			Waiting,
			TransitionOn,
			Active,
			TransitionOff,
		}

		public bool IsExclusive { get; protected set; }
		public bool HasFocus { get; private set; }

		public State ScreenState { get; set; }
		public ScreenManager ScreenManager { get; set; }

		public bool Remove { get; protected set; }

		public bool TransitionOn { get; protected set; }
		public float TransitionOnTime { get; protected set; }
		public bool TransitionOff { get; protected set; }
		public float TransitionOffTime { get; protected set; }
		public float TransitionPosition { get; protected set; }
		
		public GameScreen()
		{
			TransitionOn = TransitionOff = false;
			TransitionOnTime = TransitionOffTime = 0.0f;
			TransitionPosition = 0;

			IsExclusive = true;
			Remove = false;
		}

		public void Exit(bool allowTransition)
		{
			if (!allowTransition || TransitionOff == false)
			{
				Remove = true;
			}
			else
			{
				ScreenState = State.TransitionOff;
			}
		}

		public void Update(float deltaSeconds, bool hasFocus)
		{
			HasFocus = hasFocus;

			switch (ScreenState)
			{
				case State.Waiting:
					if (TransitionOn)
					{
						TransitionPosition = 0;
						ScreenState = State.TransitionOn;
					}
					else
					{
						TransitionPosition = 1;
						ScreenState = State.Active;
					}
					return;

				case State.TransitionOn:
					TransitionPosition += deltaSeconds / TransitionOnTime;
					if (TransitionPosition >= 1)
					{
						TransitionPosition = 1;
						ScreenState = State.Active;
					}
					break;

				case State.TransitionOff:
					TransitionPosition -= deltaSeconds / TransitionOffTime;
					if (TransitionPosition <= 0)
					{
						TransitionPosition = 0;
						Remove = true;
					}
					break;

				default:
					break;
			}
		
			Update(deltaSeconds);
		}

		public abstract void Update(float deltaSeconds);
		public abstract void Draw();
	}
}
