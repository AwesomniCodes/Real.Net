//// --------------------------------------------------------------------------------------------------------------------
//// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
////    file="DataDictionary.cs" project="FlowRx" solution="FlowRx" />
//// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
//// --------------------------------------------------------------------------------------------------------------------

//namespace Awesomni.Codes.FlowRx.DataSystem
//{
//    using System;
//    using System.Collections;
//    using System.Collections.Generic;
//    using System.Reflection;
//    using Utility.BehaviorCollections;

//    public class DataDictionary<TKey, TData> : DataSubject<ObservableDictionary<TKey, TData>>, IEnumerable<TData>
//    {
//        private bool _isReceiveUpdate;

//        internal DataDictionary(object key) : base(key, new ObservableDictionary<TKey, TData>()) { }

//        public IEnumerator<TData> GetEnumerator() { return ValueSubject.Value.Values.GetEnumerator(); }

//        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

//        public override void Notify(DataChange change)
//        {
//            if (!EqualityComparer<object>.Default.Equals(Key, change.KeyChain[0]))
//            {
//                throw new InvalidOperationException();
//            }

//            if (change.KeyChain.Count == 1)
//            {
//                //TODO: The whole dictionary gets replaced
//                base.Notify(change);
//            }
//            else
//            {
//                _isReceiveUpdate = true;

//                change = change.ForwardDown(Key);

//                DataObject childDataObject;

//                if (change.KeyChain.Count == 1 && change.ChangeType.HasFlag(DataChangeType.Created))
//                {
//                    if (change.Value.GetType() == typeof(ObservableDictionary<object, DataObject>))
//                    {
//                        childDataObject = GetOrCreateDirectory(change.KeyChain[0]);
//                    }
//                    else
//                    {
//                        MethodInfo method = GetType().GetMethod("GetOrCreate");
//                        MethodInfo generic = method.MakeGenericMethod(change.Value.GetType());
//                        childDataObject = (DataObject)generic.Invoke(this,
//                            new object[] { change.KeyChain[0], change.Value });
//                    }
//                }
//                else
//                {
//                    childDataObject = Get(change.KeyChain[0]);
//                }

//                childDataObject?.Notify(change);
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
//                _updates.OnNext(new DataChange<TData>(DataChangeType.Created, key, data.ValueSubject.Value).ForwardUp(Key));

//                data.Updates.Subscribe(change =>
//                {
//                    _updates.OnNext(change.ForwardUp(Key));
//                    if (change.ChangeType.HasFlag(DataChangeType.Remove))
//                    {
//                        ValueSubject.Value.Remove(change.KeyChain[0]);
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