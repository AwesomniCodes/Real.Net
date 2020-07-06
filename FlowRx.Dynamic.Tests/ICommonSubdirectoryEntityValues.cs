using Awesomni.Codes.FlowRx;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace Awesomni.Codes.FlowRx.Tests
{
    public interface ICommonSubdirectoryEntityValues
    {
        public IEntityValue<string> TestString { get; set; }
        public IEntityValue<int> TestInt { get; set; }
        public IEntityValue<double> TestDouble { get; set; }
        public IEntityValue<bool> TestBool { get; set; }
    }
}
