namespace UFrame.BridgeUI
{
    [System.Serializable]
    public struct TypeInfo
    {
        public System.Reflection.Assembly assemble
        {
            get
            {
                if (string.IsNullOrEmpty(assembleName))
                {
                    return null;
                }
                return System.Reflection.Assembly.Load(assembleName);
            }
        }
        public System.Type type
        {
            get
            {
                if (string.IsNullOrEmpty(typeName) || assemble == null)
                {
                    return null;
                }
                return assemble.GetType(typeName);
            }
        }

        public string assembleName;
        public string typeName;
        public TypeInfo(System.Type type)
        {
            this.assembleName = type.Assembly.ToString();
            this.typeName = type.FullName;
        }
        public void Update(System.Type type)
        {
            if (type == null)
            {
                UnityEngine.Debug.LogError("update typeinfo empty type!");
                return;
            }
            this.assembleName = type.Assembly.ToString();
            this.typeName = type.FullName;
        }
    }
}