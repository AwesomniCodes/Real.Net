// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataLogExtensions.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace FlowRx.Flows
{
    using System;
    using System.Reactive.Linq;
    using System.Reflection;
    using DataSystem;
    using log4net;

    public static class DataLogExtensions
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IObservable<string> AsLogOutput(this IObservable<DataUpdateInfo> updates, Func<DataUpdateInfo, string> logMessageSelector = null)
        {
            if (logMessageSelector == null)
            {
                logMessageSelector = dui => $"Key: {string.Join(" -> ", dui.KeyChain)}, Value: {dui.Value}, UpdateType: {dui.UpdateType}";
            }

            return updates.Select(logMessageSelector);
        }
    }
}