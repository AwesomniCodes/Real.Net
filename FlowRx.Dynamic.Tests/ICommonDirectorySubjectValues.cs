using Awesomni.Codes.FlowRx;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace FlowRx.Tests
{
    public interface ICommonDirectorySubjectValues
    {
        public ISubject<ICommonSubdirectorySubjectValues> TestDirectory { get; set; }
    }
}
