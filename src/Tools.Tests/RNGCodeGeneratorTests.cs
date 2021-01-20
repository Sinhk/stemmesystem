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
        [Fact]
        public void GeneratesCorrectLength()
        {
            foreach (var length in Enumerable.Range(1, 20))
            {
                var key = RngKeyGenerator.GenerateKey(length);
                key.Length.ShouldBe(length);
            }
        }

        [Fact]
        public void GeneratesRandomKeys()
        {
            var toGenerate = 1000;
            var length = 20;
            var keys = new List<string>(toGenerate);
            for (int i = 0; i < toGenerate; i++)
            {
                var key = RngKeyGenerator.GenerateKey(length);
                keys.ShouldNotContain(key);
                keys.Add(key);
            }
        }
    }

    public class KeyHasherTest
    {
        private KeyHasher _keyHasher;
        public KeyHasherTest()
        {
            _keyHasher = new KeyHasher();
        }

        [Fact]
        public void ValidatesOnCorrectKey()
        {
            var key = "some key";

            var hash = _keyHasher.CreateHash(key);

            _keyHasher.VerifyHash(hash, key).ShouldBeTrue();

        }

        [Fact]
        public void FailesValidationOnIncorrectKey()
        {
            var key = "some key";
            var wrongKey = "some other key";

            var hash = _keyHasher.CreateHash(key);

            _keyHasher.VerifyHash(hash, wrongKey).ShouldBeFalse();
        }


    }
}