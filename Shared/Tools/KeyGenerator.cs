using NanoidDotNet;

namespace Stemmesystem.Shared.Tools;

public class KeyGenerator
{
    private static readonly string Alphabet = Nanoid.Alphabets.NoLookAlikes;
        
    public static string GenerateKey(int length)
    {
        return Nanoid.Generate(Alphabet, length);
    }
}