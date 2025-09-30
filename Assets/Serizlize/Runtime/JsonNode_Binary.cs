using System;

namespace UFrame.Serizlize
{
#if !SimpleJSON_ExcludeBinary
    public abstract partial class JsonNode
    {
        public abstract void SerializeBinary(System.IO.BinaryWriter aWriter);

        public void SaveToBinaryStream(System.IO.Stream aData)
        {
            var W = new System.IO.BinaryWriter(aData);
            SerializeBinary(W);
        }

#if USE_SharpZipLib
		public void SaveToCompressedStream(System.IO.Stream aData)
		{
			using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
			{
				gzipOut.IsStreamOwner = false;
				SaveToBinaryStream(gzipOut);
				gzipOut.Close();
			}
		}
 
		public void SaveToCompressedFile(string aFileName)
		{
 
			System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
			using(var F = System.IO.File.OpenWrite(aFileName))
			{
				SaveToCompressedStream(F);
			}
		}
		public string SaveToCompressedBase64()
		{
			using (var stream = new System.IO.MemoryStream())
			{
				SaveToCompressedStream(stream);
				stream.Position = 0;
				return System.Convert.ToBase64String(stream.ToArray());
			}
		}
 
#else
        public void SaveToCompressedStream(System.IO.Stream aData)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }

        public void SaveToCompressedFile(string aFileName)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }

        public string SaveToCompressedBase64()
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
#endif

        public void SaveToBinaryFile(string aFileName)
        {
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
            using (var F = System.IO.File.OpenWrite(aFileName))
            {
                SaveToBinaryStream(F);
            }
        }

        public string SaveToBinaryBase64()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                SaveToBinaryStream(stream);
                stream.Position = 0;
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }

        public static JsonNode DeserializeBinary(System.IO.BinaryReader aReader)
        {
            JsonNodeType type = (JsonNodeType)aReader.ReadByte();
            switch (type)
            {
                case JsonNodeType.Array:
                    {
                        int count = aReader.ReadInt32();
                        JsonArray tmp = new JsonArray();
                        for (int i = 0; i < count; i++)
                            tmp.Add(DeserializeBinary(aReader));
                        return tmp;
                    }
                case JsonNodeType.Object:
                    {
                        int count = aReader.ReadInt32();
                        JsonObject tmp = new JsonObject();
                        for (int i = 0; i < count; i++)
                        {
                            string key = aReader.ReadString();
                            var val = DeserializeBinary(aReader);
                            tmp.Add(key, val);
                        }
                        return tmp;
                    }
                case JsonNodeType.String:
                    {
                        return new JsonString(aReader.ReadString());
                    }
                case JsonNodeType.Number:
                    {
                        return new JsonNumber(aReader.ReadDouble());
                    }
                case JsonNodeType.Boolean:
                    {
                        return new JsonBool(aReader.ReadBoolean());
                    }
                case JsonNodeType.NullValue:
                    {
                        return JsonNull.CreateOrGet();
                    }
                default:
                    {
                        throw new Exception("Error deserializing JSON. Unknown tag: " + type);
                    }
            }
        }

#if USE_SharpZipLib
		public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
		{
			var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
			return LoadFromBinaryStream(zin);
		}
		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
			using(var F = System.IO.File.OpenRead(aFileName))
			{
				return LoadFromCompressedStream(F);
			}
		}
		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
			var tmp = System.Convert.FromBase64String(aBase64);
			var stream = new System.IO.MemoryStream(tmp);
			stream.Position = 0;
			return LoadFromCompressedStream(stream);
		}
#else
        public static JsonNode LoadFromCompressedFile(string aFileName)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }

        public static JsonNode LoadFromCompressedStream(System.IO.Stream aData)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }

        public static JsonNode LoadFromCompressedBase64(string aBase64)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
#endif

        public static JsonNode LoadFromBinaryStream(System.IO.Stream aData)
        {
            using (var R = new System.IO.BinaryReader(aData))
            {
                return DeserializeBinary(R);
            }
        }

        public static JsonNode LoadFromBinaryFile(string aFileName)
        {
            using (var F = System.IO.File.OpenRead(aFileName))
            {
                return LoadFromBinaryStream(F);
            }
        }

        public static JsonNode LoadFromBinaryBase64(string aBase64)
        {
            var tmp = System.Convert.FromBase64String(aBase64);
            var stream = new System.IO.MemoryStream(tmp);
            stream.Position = 0;
            return LoadFromBinaryStream(stream);
        }
    }

    public partial class JsonArray : JsonNode
    {
        public override void SerializeBinary(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JsonNodeType.Array);
            aWriter.Write(m_List.Count);
            for (int i = 0; i < m_List.Count; i++)
            {
                m_List[i].SerializeBinary(aWriter);
            }
        }
    }

    public partial class JsonObject : JsonNode
    {
        public override void SerializeBinary(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JsonNodeType.Object);
            aWriter.Write(m_Dict.Count);
            foreach (string K in m_Dict.Keys)
            {
                aWriter.Write(K);
                m_Dict[K].SerializeBinary(aWriter);
            }
        }
    }

    public partial class JsonString : JsonNode
    {
        public override void SerializeBinary(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JsonNodeType.String);
            aWriter.Write(m_Data);
        }
    }

    public partial class JsonNumber : JsonNode
    {
        public override void SerializeBinary(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JsonNodeType.Number);
            aWriter.Write(m_Data);
        }
    }

    public partial class JsonBool : JsonNode
    {
        public override void SerializeBinary(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JsonNodeType.Boolean);
            aWriter.Write(m_Data);
        }
    }
    public partial class JsonNull : JsonNode
    {
        public override void SerializeBinary(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JsonNodeType.NullValue);
        }
    }
    internal partial class JsonCreator : JsonNode
    {
        public override void SerializeBinary(System.IO.BinaryWriter aWriter)
        {

        }
    }
#endif
}
