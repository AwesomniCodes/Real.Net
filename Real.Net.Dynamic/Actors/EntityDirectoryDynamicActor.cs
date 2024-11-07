// <copyright year="2024" holder="Awesomni.Codes" author="Felix Keil" contact="felix.keil@awesomni.codes"
//    file="EntityDirectoryDynamicActor.cs" project="Real.Net.Dynamic" solution="Real.Net" />
// <license type="Apache-2.0" ref="https://opensource.org/licenses/Apache-2.0" />
// --------------------------------------------------------------------------------------------------------------------

using Awesomni.Codes.Real.Net;
using Awesomni.Codes.Real.Net.Utility;
using ImpromptuInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace Awesomni.Codes.Real.Net.Dynamic.Actors
{
    internal class EntityDirectoryDynamicActor<TKey> : EntityDictionaryDynamicActor<TKey,IEntity>, IEntityDynamicActor
    {
        internal EntityDirectoryDynamicActor(IEntityDirectory<TKey> directory, SyntaxOptions syntaxOptions) : base(directory, syntaxOptions)
        {
        }
    }
}
