using Shouldly;
using Stemmesystem.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tools.Tests
{
    public class RNGCodeGeneratorTests
    {
        private readonly IKeyGenerator _generator;

        public RNGCodeGeneratorTests()
        {
            _generator = new RNGKeyGenerator();
        }

        [Fact]
        public void GeneratesCorrectLength()
        {
            foreach (var length in Enumerable.Range(1, 20))
            {
                var key = _generator.GenerateKey(length);
                key.Length.ShouldBe(length);
            }
        }

        [Fact]
        public void GeneratesRandomKeys()
        {
            var toGenerate = 1000;
            var length = 4;
            var keys = new List<string>(toGenerate);
            for (int i = 0; i < toGenerate; i++)
            {
                var key = _generator.GenerateKey(length);
                keys.ShouldNotContain(key);
                keys.Add(key);
            }
        }
    }
}
