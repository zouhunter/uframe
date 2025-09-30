namespace UFrame.CoolDown
{
    public interface ICoolDownChecker
    {
        float Precent { get; }
        void ResetState(bool coolEnd = false);
        bool CoolCheck(bool autoReset = true);
    }
}