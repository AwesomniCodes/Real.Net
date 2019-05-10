// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="ApplicationSystem.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace FlowRx
{
    using DataSystem;

    public class ApplicationSystem
    {
        public ApplicationSystem(object rootKey = null) => Root = new DataDirectory(rootKey);

        public DataDirectory Root { get; }
    }
}