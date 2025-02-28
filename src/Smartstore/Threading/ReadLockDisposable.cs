﻿namespace Smartstore.Threading
{
    public sealed class ReadLockDisposable : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        public ReadLockDisposable(ReaderWriterLockSlim rwLock)
        {
            this._rwLock = rwLock;
        }

        void IDisposable.Dispose()
        {
            this._rwLock.ExitReadLock();
        }
    }
}
