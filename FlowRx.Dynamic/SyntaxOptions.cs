using Awesomni.Codes.FlowRx;
using ImpromptuInterface;
using System;
using System.Dynamic;

namespace FlowRx.Dynamic
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
