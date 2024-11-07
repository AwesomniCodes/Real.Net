using System;
using System.Collections.Generic;
using System.Text;

namespace Awesomni.Codes.Real.Net.Tests
{
    public interface ICommonSubdirectoryBaseValues
    {
        public string TestString { get; set; }
        public int TestInt { get; set; }
        public double TestDouble { get; set; }
        public bool TestBool { get; set; }
        public IList<int> TestList { get; set; }
    }
}
