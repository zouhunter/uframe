using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame
{
    public class LocalizationTextCfg : TableCfg.IRow
    {
        protected int id;
        protected string text;
        public int ID => id;
        public string Text => text;

        public object GetData(string key)
        {
            throw new System.NotImplementedException();
        }

        public void SetData(string[] array)
        {
            if (array!= null)
            {
                if (array.Length > 0) id = int.Parse(array[0]);
                if (array.Length > 1) text = array[1];
            }
        }

        public void SetData(string key, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}