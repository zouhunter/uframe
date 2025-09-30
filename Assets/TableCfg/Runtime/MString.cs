using System.IO;

namespace UFrame.TableCfg
{
    public struct MString
    {
        public int pos;
        public BinaryReaderContent readerContent;
        public MString(BinaryReaderContent content,int pos)
        {
            this.readerContent = content;
            this.pos = pos;
#if !BINARY_CONFIG
            this.str = null;
#endif
        }

#if !BINARY_CONFIG
        public string str;
        public MString(string str)
        {
            this.readerContent = null;
            this.pos = 0;
            this.str = str;
        }
#endif
        public string ReadString()
        {
#if !BINARY_CONFIG
            if (str != null)
            {
                return str;
            }
            else
            {
                str = readerContent?.ReadString(pos);
                return str;
            }
#else
            return readerContent?.ReadString(pos);
#endif

        }
#if !BINARY_CONFIG
        public static implicit operator MString(string target)
        {
            return new MString(target);
        }
#endif

        public static implicit operator string(MString target)
        {
            return target.ReadString();
        }
    }
}