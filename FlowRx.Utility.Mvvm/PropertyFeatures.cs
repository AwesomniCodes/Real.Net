// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="PropertyFeatures.cs" project="FlowRx.Utility.Mvvm" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace FlowRx.Utility.Mvvm
{
    using System;

    internal class PropertyFeatures
    {
        public bool IsDynamic { get; set; }

        public bool LogOnUserSet { get; set; }

        public string Name { get; set; }

        public IObserver<object> Set { get; set; }

        public IObservable<object> Get { get; set; }

        public object LastValue { get; set; }

    }
}