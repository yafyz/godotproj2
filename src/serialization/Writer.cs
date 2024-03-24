using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Serialization;

public class Writer {
    public Stream stream;

    public Writer(Stream stream) {
        this.stream = stream;
    }

    public void WriteBytes(byte[] value) {
        stream.Write(value);
    }

    public void WriteInt32(int value) {
        var buf = new byte[sizeof(int)];
        BitConverter.TryWriteBytes(buf, value);
        stream.Write(buf);
    }

    public void WriteString(string value) {
        WriteInt32(Encoding.UTF8.GetByteCount(value));
        stream.Write(Encoding.UTF8.GetBytes(value));
    }

    public void WriteFloat(float value) {
        var buf = new byte[sizeof(float)];
        BitConverter.TryWriteBytes(buf, value);
        stream.Write(buf);
    }

    public void WriteDouble(double value) {
        var buf = new byte[sizeof(double)];
        BitConverter.TryWriteBytes(buf, value);
        stream.Write(buf);
    }

    public void WriteVector3(Vector3 value) {
        WriteFloat(value.X);
        WriteFloat(value.Y);
        WriteFloat(value.Z);
    }

    public void WriteVector3D(Vector3D value) {
        WriteDouble(value.X);
        WriteDouble(value.Y);
        WriteDouble(value.Z);
    }

    public void WriteBool(bool value) {
        stream.WriteByte(value ? (byte)1 : (byte)0);
    }

    public void WriteImage(StoredImage img)
    {
        WriteString(img.Name);
        WriteInt32((int)img.Format);
        WriteInt32(img.RawData.Length);
        WriteBytes(img.RawData);
    }
}