//*************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2022-03-16 15:46:42
//* 描    述： stream use

//* ************************************************************************************
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;

namespace Jagat.TableCfg
{
    public abstract class InUseContent : IDisposable
    {
        protected HashSet<object> inUse;
        public bool IsUsing => inUse != null && inUse.Count > 0;

        public void Dispose()
        {
            Dispose(false);
        }
        public abstract void Dispose(bool force);

        public void InUse(object target)
        {
            if (this.inUse == null)
                this.inUse = new HashSet<object>();
            this.inUse.Add(target);
        }

        public void OverUse(object target)
        {
            if (this.inUse != null)
                this.inUse.Remove(target);
            Dispose(false);
        }
    }

    public class InUseContent<T> : InUseContent where T : IDisposable
    {
        public T target;

        public InUseContent(T target)
        {
            this.target = target;
        }
        public override void Dispose(bool force)
        {
            if (this.target == null)
                return;

            if (force || inUse == null || inUse.Count <= 0)
            {
                this.target.Dispose();
                this.target = default(T);
                inUse = null;
            }
        }
    }

    public class StreamContent : InUseContent<Stream>
    {
        public StreamContent(System.IO.Stream stream) : base(stream)
        {
        }
        public StreamContent(byte[] bytes) : base(new System.IO.MemoryStream(bytes))
        {
        }

        public static implicit operator Stream(StreamContent content)
        {
            return content.target;
        }
    }

    public class BinaryReaderContent : InUseContent<BinaryReader>
    {
        private Dictionary<int, string> m_strCache = new Dictionary<int, string>();

        public BinaryReaderContent(System.IO.BinaryReader reader) : base(reader)
        {
        }
        public BinaryReaderContent(Stream stream) : base(new System.IO.BinaryReader(stream))
        {
        }

        public static implicit operator BinaryReader(BinaryReaderContent content)
        {
            return content.target;
        }

        public string ReadString(int pos)
        {
            if (m_strCache.TryGetValue(pos, out var str))
                return str;

            if (target.BaseStream != null)
            {
                this.target.BaseStream.Seek(pos, SeekOrigin.Begin);
                str = ReadString();
                m_strCache[pos] = str;
            }
            return str;
        }

        public MString ReadMString()
        {
            return UConvert.Instance.ReadMString(this);
        }
        public string ReadString()
        {
            return target.ReadString();
        }

        public virtual bool ReadBoolean()
        {
            return target.ReadBoolean();
        }
        public virtual byte ReadByte()
        {
            return target.ReadByte();
        }
        public virtual byte[] ReadBytes(int count)
        {
            return target.ReadBytes(count);
        }
        public virtual char ReadChar()
        {
            return target.ReadChar();
        }
        public virtual char[] ReadChars(int count)
        {
            return target.ReadChars(count);
        }
        public virtual decimal ReadDecimal()
        {
            return target.ReadDecimal();
        }
        public virtual double ReadDouble()
        {
            return target.ReadDouble();
        }
        public virtual short ReadInt16()
        {
            return target.ReadInt16();
        }
        public virtual int ReadInt32()
        {
            return target.ReadInt32();
        }
        public virtual long ReadInt64()
        {
            return target.ReadInt64();
        }
        public virtual sbyte ReadSByte()
        {
            return target.ReadSByte();
        }
        public virtual float ReadSingle()
        {
            return target.ReadSingle();
        }
        public virtual ushort ReadUInt16()
        {
            return target.ReadUInt16();
        }
        public virtual uint ReadUInt32()
        {
            return target.ReadUInt32();
        }
        public virtual ulong ReadUInt64()
        {
            return target.ReadUInt64();
        }
    }
}