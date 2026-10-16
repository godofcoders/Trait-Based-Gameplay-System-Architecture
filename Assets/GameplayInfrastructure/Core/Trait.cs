using System;

namespace GameplayInfrastructure.Core
{
    // One runtime instance per entity. Never put runtime state in shared config assets.
    public abstract class Trait : ITrait
    {
        private bool _used;
        private bool _disposed;
        private IAttributeContext _context;

        protected bool IsDisposed => _disposed;

        protected IAttributeContext Context
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(GetType().Name);
                return _context ?? throw new InvalidOperationException("Initialize the trait first.");
            }
        }

        public void Initialize(IAttributeContext context)
        {
            if (_used || _disposed) throw new InvalidOperationException("Traits have a single initialization lifecycle.");
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _used = true;
            try { OnInitialize(); }
            catch (Exception initializationError)
            {
                try { Dispose(); }
                catch (Exception cleanupError) { throw new AggregateException(initializationError, cleanupError); }
                throw;
            }
        }

        protected abstract void OnInitialize();
        protected virtual void OnDispose(IAttributeContext context) { }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { if (_context != null) OnDispose(_context); }
            finally { _context = null; }
        }
    }
}
