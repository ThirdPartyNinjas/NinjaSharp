using System;

namespace ThirdPartyNinjas.NinjaSharp
{
	public static class ExtensionMethods
	{
		public static float NextFloat(this Random random)
		{
			return (float)random.NextDouble();
		}
	}
}
