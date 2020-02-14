//// --------------------------------------------------------------------------------------------------------------------
//// <copyright year="2019" author="Felix Keil" contact="keil.felix@outlook.com"
////    file="DataDirectory.cs" project="Keil.FlowRx" solution="Keil.FlowRx" />
//// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
//// --------------------------------------------------------------------------------------------------------------------

//namespace Keil.FlowRx.DataSystem
//{
//    using System;
//    using System.Collections;
//    using System.Collections.Generic;
//    using System.Reflection;
//    using Utility.BehaviorCollections;

//    public class DataList<TData> : DataSubject<ObservableList<TData>>, IEnumerable<TData>
//    {
//        private bool _isReceiveUpdate;

//        internal DataList(object key) : base(key, new ObservableList<TData>()) { }

//        public IEnumerator<TData> GetEnumerator() { return ValueSubject.Value.GetEnumerator(); }

//        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

//        public override void Notify(DataUpdateInfo updateInfo)
//        {
//            if (!EqualityComparer<object>.Default.Equals(Key, updateInfo.KeyChain[0]))
//            {
//                throw new InvalidOperationException();
//            }

//            if (updateInfo.KeyChain.Count == 1)
//            {
//                //TODO: The whole list gets replaced
//                base.Notify(updateInfo);
//            }
//            else
//            {
//                _isReceiveUpdate = true;

//                updateInfo = updateInfo.ForwardDown(Key);

//                DataObject childDataObject;

//                if (updateInfo.KeyChain.Count == 1 && updateInfo.UpdateType.HasFlag(DataUpdateType.Created))
//                {
//                    if (updateInfo.Value.GetType() == typeof(ObservableDictionary<object, DataObject>))
//                    {
//                        childDataObject = GetOrCreateDirectory(updateInfo.KeyChain[0]);
//                    }
//                    else
//                    {
//                        MethodInfo method = GetType().GetMethod("GetOrCreate");
//                        MethodInfo generic = method.MakeGenericMethod(updateInfo.Value.GetType());
//                        childDataObject = (DataObject)generic.Invoke(this,
//                            new object[] { updateInfo.KeyChain[0], updateInfo.Value });
//                    }
//                }
//                else
//                {
//                    childDataObject = Get(updateInfo.KeyChain[0]);
//                }

//                childDataObject?.Notify(updateInfo);
//                _isReceiveUpdate = false;
//            }
//        }

//        private DataSubject<TData> GetOrCreateHelper<TData>(object key, Func<DataSubject<TData>> createData)
//        {
//            DataSubject<TData> data;
//            if (!ValueSubject.Value.TryGetValue(key, out var dataObject))
//            {
//                data = createData();
//                ValueSubject.Value.Add(key, dataObject = data);
//                _updates.OnNext(new DataUpdateInfo<TData>(DataUpdateType.Created, key, data.ValueSubject.Value).ForwardUp(Key));

//                data.Updates.Subscribe(updateInfo =>
//                {
//                    _updates.OnNext(updateInfo.ForwardUp(Key));
//                    if (updateInfo.UpdateType.HasFlag(DataUpdateType.Remove))
//                    {
//                        ValueSubject.Value.Remove(updateInfo.KeyChain[0]);
//                    }
//                });
//            }
//            else
//            {
//                data = (DataSubject<TData>)dataObject;
//            }

//            return data;
//        }
//    }
//}