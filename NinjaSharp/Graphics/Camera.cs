using System;

using Microsoft.Xna.Framework;

namespace ThirdPartyNinjas.NinjaSharp.Graphics
{
	public class Camera
	{
		public Point ScreenSize
		{
			get { return screenSize; }
			
			set
			{
				screenSize = value;
				screenMatrix = Matrix.CreateTranslation(screenSize.X / 2, screenSize.Y / 2, 0);
			}
		}

		public Vector2 Position
		{
			get { return position; }
			
			set
			{
				position = value;
				positionMatrix = Matrix.CreateTranslation(-position.X, -position.Y, 0);
			}
		}

		public float Angle
		{
			get { return angle; }
			
			set
			{
				angle = value;
				angleMatrix = Matrix.CreateRotationZ(-angle);
			}
		}

		public float Zoom
		{
			get { return zoom; }
			
			set
			{
				zoom = value;
				zoomMatrix = Matrix.CreateScale(zoom, zoom, 1);
			}
		}
		
		// todo: do some profiling to determine if we should cache this result
		public Matrix Transform { get { return positionMatrix * zoomMatrix * angleMatrix * screenMatrix; } }
		
		public Point TopLeft
		{
			get
			{
				return new Point((int)(position.X - screenSize.X / 2 / zoom), (int)(position.Y - screenSize.Y / 2 / zoom));
			}
		}

		public Point BottomRight
		{
			get
			{
				return new Point((int)(position.X + screenSize.X / 2 / zoom), (int)(position.Y + screenSize.Y / 2 / zoom));
			}
		}
		
		public Camera(int screenWidth, int screenHeight)
		{
			Position = new Vector2(0, 0);
			Angle = 0;
			Zoom = 1;
			ScreenSize = new Point(screenWidth, screenHeight);
		}

		Point screenSize;
		Vector2 position;
		float angle;
		float zoom;
		
		Matrix screenMatrix;
		Matrix positionMatrix;
		Matrix angleMatrix;
		Matrix zoomMatrix;
	}
}

