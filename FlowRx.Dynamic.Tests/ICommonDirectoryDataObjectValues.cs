using Awesomni.Codes.FlowRx;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowRx.Tests
{
    public interface ICommonDirectoryEntityValues
    {
        public ICommonSubdirectoryEntityValues TestDirectory { get; set; }
    }
}
