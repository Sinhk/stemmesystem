using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Stemmesystem.Tools
{
    public interface IKeyGenerator
    {
        string GenerateKey(int length);

    }
    public class RNGKeyGenerator : IKeyGenerator
    {
        private static readonly char[] chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

        string IKeyGenerator.GenerateKey(int length) => RNGKeyGenerator.GenerateKey(length);
        
        public static string GenerateKey(int length)
        {
            StringBuilder builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                var rnd = RandomNumberGenerator.GetInt32(chars.Length);
                builder.Append(chars[rnd]);
            }

            return builder.ToString();
        }
    }

    
}
