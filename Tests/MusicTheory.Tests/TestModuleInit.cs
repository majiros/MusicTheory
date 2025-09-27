using System.Runtime.CompilerServices;
using MusicTheory.Theory.Harmony;

#pragma warning disable CA2255 // 'ModuleInitializer' is intended for application code or advanced source generators

namespace MusicTheory.Tests;

internal static class TestModuleInit
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Warm up RomanInputParser and involved static paths to reduce first-run jitter
    var key = new Key(60, true);
    _ = RomanInputParser.Parse("I; V7; N6; bVI; bIII; bII; vii0/V; viio7/V; It6", key);
    }
}

#pragma warning restore CA2255
