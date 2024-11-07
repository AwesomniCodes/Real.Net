using Awesomni.Codes.Real.Net;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace Awesomni.Codes.Real.Net.Tests
{
    public interface ICommonDirectorySubjectValues
    {
        public ISubject<ICommonSubdirectorySubjectValues> TestDirectory { get; set; }
    }
}
