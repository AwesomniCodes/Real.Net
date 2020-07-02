using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace FlowRx.Tests
{
    public interface ICommonSubdirectorySubjectValues
    {
        public ISubject<string> TestString { get; set; }
        public ISubject<int> TestInt { get; set; }
        public ISubject<double> TestDouble { get; set; }
        public ISubject<bool> TestBool { get; set; }
    }
}
