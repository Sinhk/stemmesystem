using NanoidDotNet;

namespace Stemmesystem.Shared.Tools;

public static class KeyGenerator
{
    private const string Alphabet = Nanoid.Alphabets.NoLookAlikes;

    public static string GenerateKey(int length)
    {
        return Nanoid.Generate(Alphabet, length);
    }
}