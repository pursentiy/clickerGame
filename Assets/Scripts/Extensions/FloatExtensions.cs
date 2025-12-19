using System;
using UnityEngine;

namespace Extensions
{
    public static class FloatExtensions
    {
        public static bool IsEqualWithEpsilon(this float val, float comparable, float eps = Single.Epsilon)
        {
            return Mathf.Abs(val - comparable) <= eps;
        }
    }
}