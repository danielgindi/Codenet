using System;
using System.Runtime.InteropServices;

namespace Codenet.Drawing.ImageProcessing
{
    /// <summary>
    /// Credit: J. Dunlap - https://www.codeproject.com/Articles/16406/Fast-Drawing-of-Non-32bpp-Images-with-System-Drawi
    /// License: The Code Project Open License (CPOL).
    /// </summary>
    internal class SharedPinnedByteArray : IDisposable
    {
        internal byte[] Bits = null;
        internal GCHandle? Handle = null;
        internal IntPtr BitsPtr = IntPtr.Zero;
        private int _RefCount = 0;

        public SharedPinnedByteArray(int length)
        {
            Bits = new byte[length];
            Handle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            BitsPtr = Marshal.UnsafeAddrOfPinnedArrayElement(Bits, 0);
            _RefCount++;
        }

        public void AddReference()
        {
            _RefCount++;
        }
        public void ReleaseReference()
        {
            _RefCount--;
            if (_RefCount <= 0)
            {
                Dispose(false);
            }
        }
        public int GetReferenceCount()
        {
            return _RefCount;
        }

        #region IDisposable Members
        private bool _Disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed) return;

            if (_RefCount != 0) throw new Exception("Disposing an object with reference count of " + _RefCount);
            if (disposing)
            {
                // Release Managed Resources
            }
            // Now clean up Native Resources (Pointers)
            if (Handle != null)
            {
                Handle.Value.Free();
                Bits = null;
                Handle = null;
                BitsPtr = IntPtr.Zero;
            }

            _Disposed = true;
        }
        #endregion

        ~SharedPinnedByteArray()
        {
            Dispose(false);
        }
    }
}
