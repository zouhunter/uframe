namespace UFrame.BridgeUI.Editors
{
    [System.Serializable]
    public class BindingEvent
    {
        public BindingType type;
        public string bindingSource;
        public string bindingTarget;
        public TypeInfo bindingTargetType;
    }

    public enum BindingType
    {
        Simple = 0,
        Full = 1
    }
}