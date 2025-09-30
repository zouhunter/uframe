
namespace UFrame.BehaviourTree
{
    public class Status
    {
        public const byte Inactive = 0; // 未激活
        public const byte Running = 1;  // 运行中 （接续执行）
        public const byte Failure = 2;  // 失败
        public const byte Success = 3;  // 成功
        public const byte Interrupt = 4;// 中断 （重新执行）
    }
    public enum StatusE
    {
        Inactive = Status.Inactive, // 未激活
        Running = Status.Running, // 运行中 （接续执行）
        Failure = Status.Failure, // 失败
        Success = Status.Success,  // 成功
        Interrupt = Status.Interrupt// 中断 （重新执行）
    }
}
