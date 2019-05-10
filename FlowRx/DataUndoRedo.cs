// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataUndoRedo.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace FlowRx.Flows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using DataSystem;
    using DynamicData;
    using log4net;

    public class DataUndoRedo
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<IDataObject, IDisposable> _subscriptions = new Dictionary<IDataObject, IDisposable>();
        private readonly IDisposable _dataLinkSyncListSubscription;

        public DataUndoRedo()
        {
            // Sync everything, structure, values, multidirectional
            _dataLinkSyncListSubscription = UndoRedoDataLinks.Connect().Subscribe(
                changeSet =>
                {
                    foreach (var change in changeSet)
                    {
                        if (change.Reason == ListChangeReason.Add)
                        {
                            _subscriptions.Add(change.Item.Current, DataLinkUndoRedoSubscription(change.Item.Current));
                        }

                        if (change.Reason == ListChangeReason.Remove)
                        {
                            _subscriptions[change.Item.Current]?.Dispose();
                            _subscriptions.Remove(change.Item.Current);
                        }
                    }
                });
        }

        public SourceList<IDataObject> UndoRedoDataLinks { get; } = new SourceList<IDataObject>();

        private IDisposable DataLinkUndoRedoSubscription(IDataObject dataLink)
            => dataLink.Link.Where(dui => !dui.UpdateType.HasFlag(DataUpdateType.Sync)).Subscribe(dui =>
            {
                dui = dui.CreateWithSameType(dui.UpdateType | DataUpdateType.Sync, dui.KeyChain, dui.Value);

                foreach (var linkToNotify in UndoRedoDataLinks.Items.Where(item => item != dataLink))
                {
                    linkToNotify.Link.OnNext(dui);
                }
            });
    }
}