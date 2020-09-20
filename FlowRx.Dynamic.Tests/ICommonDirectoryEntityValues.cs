using Awesomni.Codes.FlowRx;
using Awesomni.Codes.FlowRx.Dynamic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Awesomni.Codes.FlowRx.Tests
{
    public interface ICommonDirectoryEntityValues
    {
        public IEntityDynamic<ICommonSubdirectoryEntityValues> TestDirectory { get; set; }
    }
}
