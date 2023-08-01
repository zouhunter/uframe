//*************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2022-03-16 15:44:14
//* 描    述： 

//* ************************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jagat.TableCfg
{
    public interface IProxyTable:IDisposable
    {
        List<string> FileNames { get; }
        ITable Table { get; }
        bool Finished { get; }
        void SetLoader(ITextLoader tableLoader);
        void SetDecoder(byte style,ITableDecoder decoder);
        AsyncTableOperation StartLoad(Action<string, bool> onLoad);
        int GetTableNum();
    }
}