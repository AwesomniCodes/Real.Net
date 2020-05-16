using Awesomni.Codes.FlowRx;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowRx.Tests
{
    public interface ICommonDirectoryBaseValues
    {
        public ICommonSubdirectoryBaseValues TestDirectory { get; set; }
    }
}
