using System.Threading;
using System.Threading.Tasks;

namespace Novike.Threading
{
    public class DisposableCancellationTokenSource : CancellationTokenSource
    {
        public bool IsDisposed => _disposed;

        private CancellationTokenSource _cts;
        private bool _disposed;

        public DisposableCancellationTokenSource()
        {
            _cts = new CancellationTokenSource();
            _disposed = false;
        }
        private DisposableCancellationTokenSource(CancellationTokenSource linkedCts)
        {
            _cts = linkedCts;
            _disposed = false;
        }

        new public CancellationToken Token => _cts.Token;

        new public static DisposableCancellationTokenSource CreateLinkedTokenSource(CancellationToken[] tokens)
        {
            return new DisposableCancellationTokenSource(CancellationTokenSource.CreateLinkedTokenSource(tokens));
        }
        
        new public static DisposableCancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2)
        {
            return new DisposableCancellationTokenSource(CancellationTokenSource.CreateLinkedTokenSource(token1, token2));
        }

        public static DisposableCancellationTokenSource CreateLinkedTokenSource(CancellationToken token)
        {
            return new DisposableCancellationTokenSource(CancellationTokenSource.CreateLinkedTokenSource(token));
        }

        public bool TryCancel()
        {
            if (_disposed == false)
            {
                _cts.Cancel();
                return true;
            }

            return false;
        }

        public bool TryDispose()
        {
            if (_disposed == false)
            {
                _cts.Dispose();
                _disposed = true;
                return true;
            }

            return false;
        }
    }
}
