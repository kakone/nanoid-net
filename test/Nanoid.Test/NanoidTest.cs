using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NanoidDotNet.Test
{
    public class NanoidTest
    {
        private const int DefaultSize = 21;
        private const string DefaultAlphabet = "_-0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        [Fact]
        public void TestDefault()
        {
            var result = Nanoid.Generate();
            Assert.Equal(DefaultSize, result.Length);
        }

        [Fact]
        public void TestCustomSize()
        {
            var result = Nanoid.Generate(size:10);
            Assert.Equal(10, result.Length);
        }
        [Fact]
        public void TestCustomAlphabet()
        {
            var result = Nanoid.Generate("1234abcd");
            Assert.Equal(DefaultSize, result.Length);
        }

        [Fact]
        public void TestCustomAlphabetAndSize()
        {
            var result = Nanoid.Generate("1234abcd", 7);
            Assert.Equal(7, result.Length);
        }

        [Fact]
        public void TestCustomRandom()
        {
            var random = new Random(10);
            var result = Nanoid.Generate(random);
            Assert.Equal(DefaultSize, result.Length);
        }

        [Fact]
        public void TestSingleLetterAlphabet()
        {
            var actual = Nanoid.Generate("a", 5);

            Assert.Equal("aaaaa", actual);
        }

        [Theory]
        [InlineData(4, "adca")]
        [InlineData(18, "cbadcbadcbadcbadcc")]
        public void TestPredefinedRandomSequence(int size, string expected)
        {
            byte[] sequence = { 2, 255, 3, 7, 7, 7, 7, 7, 0, 1 };
            var random = new PredefinedRandomSequence(sequence);
            var actual = Nanoid.Generate(random, "abcde", size);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task TestAsyncGenerate()
        {

            var result = await Nanoid.GenerateAsync();

            Assert.Equal(DefaultSize, result.Length);
        }

        [Fact]
        public void TestGeneratesUrlFriendlyIDs()
        {
            foreach(var dummy in Enumerable.Range(1, 10))
            {
                var result = Nanoid.Generate();
                Assert.Equal(DefaultSize, result.Length);

                foreach(var c in result)
                {
                    Assert.Contains(c, result);
                }
            }
        }

        [Fact]
        public void TestHasNoCollisions()
        {
            const int count = 100 * 1000;
            var dictUsed = new Dictionary<string, bool>();
            foreach(var dummy in Enumerable.Range(1, count))
            {
                var result = Nanoid.Generate();
                Assert.False(dictUsed.TryGetValue(result, out var _));
                dictUsed.Add(result, true);
            }
        }

        [Fact]
        public void TestFlatDistribution()
        {
            const int count = 100 * 1000;
            var chars = new Dictionary<char, int>();
            foreach (var dummy in Enumerable.Range(1, count))
            {
                var id = Nanoid.Generate();
                for (var i = 0; i < DefaultSize; i++)
                {
                    var c = id[i];
                    if (!chars.ContainsKey(c))
                    {
                        chars.Add(c, 0);
                    }

                    chars[c] += 1;
                }

            }

            foreach (var c in chars)
            {
                var distribution = c.Value * DefaultAlphabet.Length / (double)(count * DefaultSize);
                Assert.True(ToBeCloseTo(distribution, 1, 1));
            }
        }

        [Fact]
        public void TestMask()
        {
            for (var length = 1; length < 256; length++)
            {
                var mask1 = (2 << (int)Math.Floor(Math.Log(length - 1) / Math.Log(2))) - 1;
                #if NET7_0_OR_GREATER
                var mask2 = (2 << 31 - Int32.LeadingZeroCount((length - 1) | 1)) - 1;
                #else
                var mask2 = (2 << 31 - Nanoid.Clz32((length - 1) | 1)) - 1;
                #endif
                Assert.Equal(mask1, mask2);
            }
        }

        private static bool ToBeCloseTo(double actual, double expected, int precision = 2)
        {
            var pass = Math.Abs(expected-actual) < Math.Pow(10, -precision)/2;
            return pass;
        }
    }
}