using Awesomni.Codes.FlowRx;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace Awesomni.Codes.FlowRx.Tests
{
    public interface ICommonSubdirectoryEntityValues
    {
        public IEntitySubject<string> TestString { get; set; }
        public IEntitySubject<int> TestInt { get; set; }
        public IEntitySubject<double> TestDouble { get; set; }
        public IEntitySubject<bool> TestBool { get; set; }
        public IEntityList<IEntitySubject<int>> TestList { get; set; }
    }
}
