using Awesomni.Codes.Real.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Awesomni.Codes.Real.Net.Tests
{
    public interface ICommonDirectoryBaseValues
    {
        public ICommonSubdirectoryBaseValues TestDirectory { get; set; }
    }
}
