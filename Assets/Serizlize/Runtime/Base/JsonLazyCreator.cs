using System.Text;

namespace UFrame.Serizlize
{
    internal partial class JsonCreator : JsonNode
    {
        private JsonNode m_Node = null;
        private string m_Key = null;
        public override JsonNodeType Tag { get { return JsonNodeType.None; } }
        public override Enumerator GetEnumerator() { return new Enumerator(); }

        public JsonCreator(JsonNode aNode)
        {
            m_Node = aNode;
            m_Key = null;
        }

        public JsonCreator(JsonNode aNode, string aKey)
        {
            m_Node = aNode;
            m_Key = aKey;
        }

        private T Set<T>(T aVal) where T : JsonNode
        {
            if (m_Key == null)
                m_Node.Add(aVal);
            else
                m_Node.Add(m_Key, aVal);
            m_Node = null; // Be GC friendly.
            return aVal;
        }

        public override JsonNode this[int aIndex]
        {
            get { return new JsonCreator(this); }
            set { Set(new JsonArray()).Add(value); }
        }

        public override JsonNode this[string aKey]
        {
            get { return new JsonCreator(this, aKey); }
            set { Set(new JsonObject()).Add(aKey, value); }
        }

        public override void Add(JsonNode aItem)
        {
            Set(new JsonArray()).Add(aItem);
        }

        public override void Add(string aKey, JsonNode aItem)
        {
            Set(new JsonObject()).Add(aKey, aItem);
        }

        public static bool operator ==(JsonCreator a, object b)
        {
            if (b == null)
                return true;
            return System.Object.ReferenceEquals(a, b);
        }

        public static bool operator !=(JsonCreator a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return true;
            return System.Object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override int AsInt
        {
            get { Set(new JsonNumber(0)); return 0; }
            set { Set(new JsonNumber(value)); }
        }

        public override float AsFloat
        {
            get { Set(new JsonNumber(0.0f)); return 0.0f; }
            set { Set(new JsonNumber(value)); }
        }

        public override double AsDouble
        {
            get { Set(new JsonNumber(0.0)); return 0.0; }
            set { Set(new JsonNumber(value)); }
        }

        public override long AsLong
        {
            get
            {
                if (longAsString)
                    Set(new JsonString("0"));
                else
                    Set(new JsonNumber(0.0));
                return 0L;
            }
            set
            {
                if (longAsString)
                    Set(new JsonString(value.ToString()));
                else
                    Set(new JsonNumber(value));
            }
        }

        public override ulong AsULong
        {
            get
            {
                if (longAsString)
                    Set(new JsonString("0"));
                else
                    Set(new JsonNumber(0.0));
                return 0L;
            }
            set
            {
                if (longAsString)
                    Set(new JsonString(value.ToString()));
                else
                    Set(new JsonNumber(value));
            }
        }

        public override bool AsBool
        {
            get { Set(new JsonBool(false)); return false; }
            set { Set(new JsonBool(value)); }
        }

        public override JsonArray AsArray
        {
            get { return Set(new JsonArray()); }
        }

        public override JsonObject AsObject
        {
            get { return Set(new JsonObject()); }
        }
        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
        {
            aSB.Append("null");
        }
    }
    // End of JSONLazyCreator

}
