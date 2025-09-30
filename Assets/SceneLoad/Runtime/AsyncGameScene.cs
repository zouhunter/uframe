//*************************************************************************************
//* 作    者： 
//* 创建时间： 2021-08-08 09:48:00
//* 描    述：  

//* ************************************************************************************
namespace UFrame.SceneLoad
{
    using System.Collections.Generic;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// 异步场景场景模板
    /// <summary>
    public class AsyncGameScene<T> : AsyncGameSceneGeneric<int,T> where T : AsyncGameScene<T>
    {
    }
}