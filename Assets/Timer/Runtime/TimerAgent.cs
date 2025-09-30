// ██╗░░░██╗███████╗██████╗    ░████╗  ░███╗░░░███╗███████╗
// ██║░░░██║██╔════╝██╔══██╗ ██╔═██╗░████╗░████║██╔════╝
// ██║░░░██║█████╗░░██████╔╝ ██████║░██╔████╔██║█████╗░░
// ██║░░░██║██╔══╝░░██╔══██╗ ██╔══██╗██║╚██╔╝██║██╔══╝░░
// ╚██████╔╝██║░░░░░██║░░██║ ██║░░██║██║░╚═╝░██║███████╗
// ░╚═════╝░╚═╝░░░░░╚═╝░░╚═╝ ╚═╝░░╚═╝╚═╝░░░░░╚═╝╚══════╝
//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-05-02
//* 描    述： 定时器代理，便捷调用静态方法
//* ************************************************************************************
//using System;
//using UFrame;
//using UFrame.Timer;
//public class TimerAgent : AgentContext<TimerAgent, SaftyTimerCtrl>,ILateUpdate, IUpdate, IFixedUpdate
//{
//    public bool Runing => Instance.Runing;

//    public float Interval => Instance.Interval;

//    protected override SaftyTimerCtrl CreateAgent()
//    {
//        return new SaftyTimerCtrl();
//    }

//    protected override void OnInitialize()
//    {
//        Instance.Init();
//    }

//    protected override void OnRecover()
//    {
//        Instance.Release();
//    }

//    public static void DelySecond(Action action, UFrame.IAlive alive = null)
//    {
//        if (alive == null)
//            Instance.DelyExecute(action, 1);
//        else
//            Instance.DelyExecuteSafty(alive, action, 1);
//    }

//    public static void DelyFrame(Action action, UFrame.IAlive alive = null)
//    {
//        if (alive == null)
//            Instance.DelyExecute(action, 0);
//        else
//            Instance.DelyExecuteSafty(alive, action, 0);
//    }

//    public static void StopTimer(ref int timer)
//    {
//        if (timer > 0)
//            Instance.StopTimer(timer);
//        timer = 0;
//    }

//    public static int RegistLoopFrame(Func<bool> callback, UFrame.IAlive alive = null)
//    {
//        if (alive == null)
//            return Instance.RegistLoop(callback, 0);
//        else
//            return Instance.RegistLoopSafty(alive, callback, 0);
//    }
//    public static int StartLoopFrame(Func<bool> callback, UFrame.IAlive alive = null)
//    {
//        if (alive == null)
//            return Instance.StartLoop(callback, 0);
//        else
//            return Instance.StartLoopSafty(alive, callback, 0);
//    }

//    public void OnLateUpdate()
//    {
//        Instance.OnLateUpdate();
//    }

//    public void OnUpdate()
//    {
//        Instance.OnUpdate();
//    }

//    public void OnFixedUpdate()
//    {
//        Instance.OnFixedUpdate();
//    }}


