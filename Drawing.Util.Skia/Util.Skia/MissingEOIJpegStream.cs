using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Codenet.Drawing.Util.Skia;

internal class MissingEOIJpegStream : Stream
{
    private readonly Stream _baseStream;
    private readonly byte[] _virtualBytes;
    private Int64 _position;

    public MissingEOIJpegStream(Stream baseStream)
    {
        _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
        _virtualBytes = new byte[] { 0xff, 0xd9 };
        _position = 0;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        Int64 baseLength = _baseStream.Length;
        if (_position < baseLength)
        {
            _baseStream.Position = _position;
            int readFromBase = _baseStream.Read(buffer, offset, count);
            _position += readFromBase;
            return readFromBase;
        }
        else
        {
            int virtualOffset = (int)(_position - baseLength);
            int available = _virtualBytes.Length - virtualOffset;
            if (available <= 0) return 0;

            int toCopy = Math.Min(count, available);
            Array.Copy(_virtualBytes, virtualOffset, buffer, offset, toCopy);
            _position += toCopy;
            return toCopy;
        }
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        Int64 baseLength = _baseStream.Length;
        if (_position < baseLength)
        {
            _baseStream.Position = _position;
            int readFromBase = await _baseStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            _position += readFromBase;
            return readFromBase;
        }
        else
        {
            int virtualOffset = (int)(_position - baseLength);
            int available = _virtualBytes.Length - virtualOffset;
            if (available <= 0) return 0;

            int toCopy = Math.Min(count, available);
            Array.Copy(_virtualBytes, virtualOffset, buffer, offset, toCopy);
            _position += toCopy;
            return toCopy;
        }
    }

#if NET6_0_OR_GREATER
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Int64 baseLength = _baseStream.Length;
        if (_position < baseLength)
        {
            _baseStream.Position = _position;
            int readFromBase = await _baseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            _position += readFromBase;
            return readFromBase;
        }
        else
        {
            int virtualOffset = (int)(_position - baseLength);
            int available = _virtualBytes.Length - virtualOffset;
            if (available <= 0) return 0;

            int toCopy = Math.Min(buffer.Length, available);
            _virtualBytes.AsMemory(virtualOffset, toCopy).CopyTo(buffer);
            _position += toCopy;
            return toCopy;
        }
    }
#endif

    public override Int64 Seek(Int64 offset, SeekOrigin origin)
    {
        Int64 newPos;
        switch (origin)
        {
            case SeekOrigin.Begin:
                newPos = offset;
                break;
            case SeekOrigin.Current:
                newPos = _position + offset;
                break;
            case SeekOrigin.End:
                newPos = Length + offset;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }

        if (newPos < 0)
            throw new IOException("Attempted to seek before beginning of stream");

        _position = newPos;
        return _position;
    }

    public override Int64 Position
    {
        get => _position;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Position cannot be negative");
            _position = value;
        }
    }

    public override Int64 Length => _baseStream.Length + _virtualBytes.Length;

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;

    public override void Flush() => _baseStream.Flush();
    public override void SetLength(Int64 value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _baseStream.Dispose();
        }
        base.Dispose(disposing);
    }
}

