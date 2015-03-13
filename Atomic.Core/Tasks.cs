using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Core
{
    public interface IFileTask : ITask { }

    public interface IGeneralTask : ITask { }

    public class FileTask : AtomicTask { }

    public class GeneralTask : AtomicTask { }
}
