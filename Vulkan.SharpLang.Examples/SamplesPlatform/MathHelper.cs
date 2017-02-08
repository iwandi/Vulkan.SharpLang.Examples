using System;
using System.Numerics;

namespace Vulkan.SharpLang.Examples
{
	public static class MathHelper
	{
		public const float PI = (float)Math.PI;
		public const float DegToRad = (PI * 2f) / 360f;
		public const float RadToDeg = 360f / (PI * 2f);

		public static float ToRadians(float value)
		{
			return DegToRad * value;
		}

		public static float ToDeg(float value)
		{
			return RadToDeg * value;
		}
	}
}
