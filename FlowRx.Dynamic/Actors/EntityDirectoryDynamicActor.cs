// <copyright year="2020" holder="Awesomni.Codes" author="Felix Keil" contact="keil.felix@outlook.com"
//    file="EntityDirectoryDynamicActor.cs" project="FlowRx.Dynamic" solution="FlowRx" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Utility;
using ImpromptuInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace Awesomni.Codes.FlowRx.Dynamic.Actors
{
    internal class EntityDirectoryDynamicActor<TKey> : EntityDictionaryDynamicActor<TKey,IEntity>, IEntityDynamicActor
    {
        internal EntityDirectoryDynamicActor(IEntityDirectory<TKey> directory, SyntaxOptions syntaxOptions) : base(directory, syntaxOptions)
        {
        }
    }
}
