using System;

namespace GameplayInfrastructure.Core
{
    public static class Numeric
    {
        public static void RequireFinite(float value, string parameter)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameter, "Value must be finite.");
        }

        public static void RequireNonNegative(float value, string parameter)
        {
            RequireFinite(value, parameter);
            if (value < 0f) throw new ArgumentOutOfRangeException(parameter, "Value must be non-negative.");
        }
    }
}
