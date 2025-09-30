//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-08-17 04:59:20
//* 描    述： 引导系统

//* ************************************************************************************
using System;
using UnityEngine;

namespace UFrame.Guide
{
    /// <summary>
    /// 触发器
    /// <summary>
    public interface IFactor
    {
        bool Process();
    }

    public interface ISkipAble
    {
        void Skip();
    }

    public interface IUndoAble
    {
        void Undo();
    }
}