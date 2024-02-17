using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Codenet.IO;

public class CsvReader : IDisposable
{
    private StreamReader StreamReader = null;
    private Stream _Stream = null;
    private bool _MultilineSupport = true;
    private bool _LastLineEndCR = false;
    private SeparatorTypeOptions _SeparatorType = SeparatorTypeOptions.Auto;
    private char _Separator = ',';
    private bool _SeparatorDetected = false;

    public CsvReader(Stream inputStream)
    {
        _Stream = inputStream;
    }

    public CsvReader(Stream inputStream, bool multilineSupport)
    {
        _Stream = inputStream;
        _MultilineSupport = multilineSupport;
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
            if (StreamReader != null)
            {
                StreamReader.Dispose();
            }
            StreamReader = null;
            if (_Stream != null)
            {
                _Stream.Dispose();
            }
            _Stream = null;
        }
        // Now clean up Native Resources (Pointers)
    }

    public Stream Stream
    {
        get { return _Stream; }
    }

    public bool MultilineSupport
    {
        get { return _MultilineSupport; }
        set { _MultilineSupport = value; }
    }

    public SeparatorTypeOptions SeparatorType
    {
        get { return _SeparatorType; }
        set
        {
            _SeparatorType = value;

            if (value == SeparatorTypeOptions.Auto)
            {
                _SeparatorDetected = false;
            }
            else if (value == SeparatorTypeOptions.Comma)
            {
                _SeparatorDetected = true;
                _Separator = ',';
            }
            else if (value == SeparatorTypeOptions.Tab)
            {
                _SeparatorDetected = true;
                _Separator = '\t';
            }
        }
    }

    /// <summary>
    /// Will read the next row
    /// </summary>
    /// <returns>Array of columns if successful and there's something to read. Otherwise <value>null</value></returns>
    public string[] ReadRow()
    {
        if (null == _Stream) return null;
        lock (this)
        {
            if (null == StreamReader)
            {
                StreamReader = new StreamReader(_Stream, Encoding.Default, true);
            }

            if (StreamReader.EndOfStream)
            {
                return null;
            }

            List<string> columns = new List<string>();
            var sbColumn = new StringBuilder();

            if (_MultilineSupport)
            {
                bool isQuoted = false;

                while (!StreamReader.EndOfStream)
                {
                    char c = (char)StreamReader.Read();

                    if (c == '\n' || c == '\r')
                    {
                        if (!isQuoted)
                        {
                            if (_LastLineEndCR && c == '\n')
                            {
                                _LastLineEndCR = false;
                                continue;
                            }
                            _LastLineEndCR = c == '\r';
                            break;
                        }
                    }

                    if (isQuoted)
                    {
                        if (c == '"')
                        {
                            if (!StreamReader.EndOfStream && (char)StreamReader.Peek() == '"')
                            {
                                sbColumn.Append('"');
                                StreamReader.Read(); // Skip next quote mark (")
                            }
                            else
                            {
                                isQuoted = false;
                            }
                        }
                        else
                        {
                            sbColumn.Append(c);
                        }
                    }
                    else
                    {
                        if (!_SeparatorDetected)
                        {
                            if (c == ',' || c == '\t')
                            {
                                _SeparatorDetected = true;
                                _Separator = c;
                            }
                        }

                        if (c == _Separator)
                        {
                            columns.Add(sbColumn.ToString());
                            sbColumn.Clear();
                        }
                        else if (c == '"')
                        {
                            if (sbColumn.Length > 0)
                            {
                                sbColumn.Append(c);
                            }
                            else
                            {
                                isQuoted = true;
                            }
                        }
                        else
                        {
                            sbColumn.Append(c);
                        }
                    }
                }
            }
            else
            {
                string line;
                char c;
                bool isQuoted;

                line = StreamReader.ReadLine();
                isQuoted = false;
                for (int j = 0, len = line.Length; j < len; j++)
                {
                    c = line[j];
                    if (isQuoted)
                    {
                        if (c == '"')
                        {
                            if (line.Length > j + 1 && line[j + 1] == '"')
                            {
                                sbColumn.Append('"');
                                j++;
                            }
                            else
                            {
                                isQuoted = false;
                            }
                        }
                        else
                        {
                            sbColumn.Append(c);
                        }
                    }
                    else
                    {
                        if (!_SeparatorDetected)
                        {
                            if (c == ',' || c == '\t')
                            {
                                _SeparatorDetected = true;
                                _Separator = c;
                            }
                        }

                        if (c == _Separator)
                        {
                            columns.Add(sbColumn.ToString());
                            sbColumn.Clear();
                        }
                        else if (c == '"')
                        {
                            if (sbColumn.Length > 0)
                            {
                                sbColumn.Append(c);
                            }
                            else
                            {
                                isQuoted = true;
                            }
                        }
                        else
                        {
                            sbColumn.Append(c);
                        }
                    }
                }
            }

            if (columns.Count > 0 || sbColumn.Length > 0 || !StreamReader.EndOfStream)
            {
                // We have a row, return it

                columns.Add(sbColumn.ToString());

                return columns.ToArray();
            }
            else
            {
                // We just ran into a newline at the end of the file, ignore it
                return null;
            }
        }
    }

    public enum SeparatorTypeOptions
    {
        Auto,
        Comma,
        Tab
    }
}
