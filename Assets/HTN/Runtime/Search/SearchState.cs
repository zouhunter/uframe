using System.Collections;
//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-06
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using System;

namespace UFrame.HTN
{
    [Serializable]
    public class SearchState
    {
        public bool isDirty;//是否需要重新查询

        public bool blackboardChanged;

        public bool planDone;

        public bool planFailed;

        public bool isSearchingAsync;

        public bool hasPlanChanged;
        public SearchResult result { get; private set; }
        public SearchState(Planner planNodePool)
        {
            isDirty = true;
            result = new SearchResult(planNodePool);
        }

        public void Clear()
        {
            isDirty = false;
            blackboardChanged = false;
            planDone = false;
            planFailed = false;
        }
    }
}

