using System;
using System.IO;
using System.Text;
using Godot;

namespace Serialization;

public class Reader {
    public Stream stream;

    public Reader(Stream stream) {
        this.stream = stream;
    }

    public byte[] ReadBytes(int length) {
        var buf = new byte[length];
        stream.Read(buf);
        return buf;
    }

    public int ReadInt32() {
        var buf = new byte[sizeof(int)];
        stream.Read(buf);
        return BitConverter.ToInt32(buf);
    }

    public string ReadString() {
        var len = ReadInt32();
        var buf = new byte[len];
        stream.Read(buf);
        return Encoding.UTF8.GetString(buf);
    }

    public float ReadFloat() {
        var buf = new byte[sizeof(float)];
        stream.Read(buf);
        return BitConverter.ToSingle(buf);
    }

    public double ReadDouble() {
        var buf = new byte[sizeof(double)];
        stream.Read(buf);
        return BitConverter.ToDouble(buf);
    }

    public Vector3 ReadVector3() {
        return new Vector3() {
            X = ReadFloat(),
            Y = ReadFloat(),
            Z = ReadFloat()
        };
    }

    public Vector3D ReadVector3D() {
        return new Vector3D() {
            X = ReadDouble(),
            Y = ReadDouble(),
            Z = ReadDouble()
        };
    }

    public bool ReadBool() {
        var v = stream.ReadByte();
        if (v == -1)
            throw new ArgumentException("EOF");
        return v != 0;
    }
}