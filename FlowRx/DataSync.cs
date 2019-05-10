// --------------------------------------------------------------------------------------------------------------------
// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="DataSync.cs" project="FlowRx" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

namespace FlowRx.Flows
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using DataSystem;
    using DynamicData;
    using log4net;

    public class DataSync
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDisposable _objectsSyncListSubscription;

        public DataSync()
        {
            // Sync everything, structure, values, multidirectional
            _objectsSyncListSubscription = SyncObjects.Connect()
                .MergeMany(dataObject => dataObject.Link.Select(dui => (DataUpdateInfo: dui, DataObject: dataObject)))
                .Where(update => !update.DataUpdateInfo.UpdateType.HasFlag(DataUpdateType.Sync))
                .Subscribe(update =>
                    {
                        var dataUpdateInfo = update.DataUpdateInfo.CreateWithSameType(update.DataUpdateInfo.UpdateType | DataUpdateType.Sync, update.DataUpdateInfo.KeyChain,
                            update.DataUpdateInfo.Value);

                        foreach (var objectToNotify in SyncObjects.Items.Where(item => item != update.DataObject))
                        {
                            objectToNotify.Link.OnNext(dataUpdateInfo);
                        }
                    }
                );
        }

        public SourceList<IDataObject> SyncObjects { get; } = new SourceList<IDataObject>();
    }
}