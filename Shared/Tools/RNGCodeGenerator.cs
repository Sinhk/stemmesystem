using System.Security.Cryptography;
using System.Text;

namespace Stemmesystem.Core.Tools
{
    public enum KeyType{SimpleAlphanumeric,FullAlphanumeric}
    public interface IKeyGenerator
    {
        string GenerateKey(int length, KeyType type = KeyType.SimpleAlphanumeric);
    }
    public class RngKeyGenerator : IKeyGenerator
    {
        private static readonly char[] CharsSimple = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
        private static readonly char[] CharsFull = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        string IKeyGenerator.GenerateKey(int length, KeyType type) => GenerateKey(length,  type);
        
        public static string GenerateKey(int length, KeyType type = KeyType.SimpleAlphanumeric)
        {
            var chars = type switch
            {
                KeyType.SimpleAlphanumeric => CharsSimple
                , KeyType.FullAlphanumeric => CharsFull
                , _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            
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
