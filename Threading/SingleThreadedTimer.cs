using System;
using System.Threading;

namespace Codenet.Threading;

public class SingleThreadedTimer : IDisposable
{
    public SingleThreadedTimer()
    {
    }

    /// <summary>
    /// Will Start() automatically.
    /// </summary>
    /// <param name="Milliseconds">Time for each Timer rotation</param>
    /// <param name="Elapsed">Event to call when Timer elapses</param>
    public SingleThreadedTimer(int Milliseconds, EventHandler Elapsed)
    {
        this.Milliseconds = Milliseconds;
        this.Elapsed += Elapsed;
        Start();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Stop();
        }
        // Now clean up Native Resources (Pointers)
    }

    private int _Milliseconds = 0;
    private Thread thread = null;
    private DateTime lastElapsed = DateTime.MaxValue;
    private bool _IsStarted = false;

    public event EventHandler Elapsed;

    public int Milliseconds
    {
        get { return _Milliseconds; }
        set { _Milliseconds = value; }
    }
    public bool IsStarted
    {
        get { return _IsStarted; }
        set { _IsStarted = value; }
    }

    public void Start()
    {
        lock (this)
        {
            if (thread != null) return;
            _IsStarted = true;
            thread = new Thread(new ThreadStart(StartAction));
            thread.Start();
        }
    }
    private bool Sleep()
    {
        if (Milliseconds > 0 && thread != null)
        {
            int MS;
            MS = Milliseconds - (DateTime.UtcNow - lastElapsed).Milliseconds;
            if (MS < 0) MS = 0;
            else if (MS > Milliseconds) MS = Milliseconds;
            Thread.Sleep(MS);
            return true;
        }
        return false;
    }
    private void StartAction()
    {
        while (Sleep())
        {
            lastElapsed = DateTime.UtcNow;
            if (Elapsed != null)
            {
                Elapsed(this, null);
            }
        }
        lock (this)
        {
            _IsStarted = false;
            thread = null;
        }
    }

    public void Stop()
    {
        lock (this)
        {
            var curThread = thread;
            thread = null;
            _IsStarted = false;
            if (curThread != null)
            {
                curThread.Abort();
            }
        }
    }
}
