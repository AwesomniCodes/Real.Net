// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="ChangeType.cs" project="Real.Net" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace Awesomni.Codes.Real.Net
{
    using System;

    [Flags]
    public enum ChangeType : int
    {
        Definition = 1,
        Modification = 2,
        Completion = 4,
        Error = 8,
    }
}