using Awesomni.Codes.Real.Net;
using ImpromptuInterface;
using System;
using System.Dynamic;

namespace Awesomni.Codes.Real.Net.Dynamic
{
    [Flags]
    public enum SyntaxOptions
    {
        DefaultAccess = 0,
        BaseMemberAccess = 1,
        MixedAccess = 2,
        AutoCreate = 4,
    }
}
