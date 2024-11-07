using Awesomni.Codes.Real.Net;
using Awesomni.Codes.Real.Net.Dynamic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Awesomni.Codes.Real.Net.Tests
{
    public interface ICommonDirectoryEntityValues
    {
        public IEntityDynamic<ICommonSubdirectoryEntityValues> TestDirectory { get; set; }
    }
}
