using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClouderSync
{
    public abstract class TaskControl
    {
        internal static CancellationTokenSource _cancelTokenSrc = null;
        internal static CancellationToken _cancelToken = new CancellationToken();
        internal static long iRunning = 0;

    }
}
