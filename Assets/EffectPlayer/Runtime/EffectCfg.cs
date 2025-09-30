using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.TableCfg
{
    [System.Serializable]
    public class EffectCfg : IRow<int>
    {
        public int K1 => id;
        /*0*/
        public int id { get; private set; } //EffectCfg
        /*1*/
        public string name { get; private set; } //单位名称
        /*2*/
        public string source { get; private set; } //资源路径
        /*3*/
        public int duration { get; private set; } //持续时间(ms)
        /*4*/
        public int sfx_id { get; private set; } //音效id

        public object GetData(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            switch (key)
            {
                case "id": return id;
                case "name": return name;
                case "source": return this.source;
                case "duration": return this.duration;
                case "sfx_id": return this.sfx_id;
            }
            return null;
        }
        public void SetData(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            switch (key)
            {
                case "id":
                    id = Convert.ToInt32(value);
                    break;
                case "name":
                    name = value;
                    break;
                case "source":
                    source = value;
                    break;
                case "duration":
                    duration = Convert.ToInt32(value);
                    break;
                case "sfx_id":
                    sfx_id = Convert.ToInt32(value);
                    break;
            }
        }
    }
}
