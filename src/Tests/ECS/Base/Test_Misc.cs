using System.Collections.Generic;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Base
{
    public static class Test_Misc
    {
        [Test]
        public static void X_Dictionary_Perf()
        {

            var count = 10; // 1_000_000;
            var dict = new Dictionary<int, int>(count);
            for (var n = 0; n < count; n++)
            {
                dict.Add(n, n);
            }
            AreEqual(count, dict.Count);
            for (var o = 0; o < 1000; o++)
            {
                for (var n = 0; n < count; n++)
                {
                    _ = dict[n];
                }
            }
        }

        [Test]
        public static void X_Array_Perf()
        {

            var count = 10; // 1_000_000;
            var array = new int[count];
            for (var o = 0; o < 1000; o++)
            {
                for (var n = 0; n < count; n++)
                {
                    _ = array[n];
                }
            }
        }
    }
}
