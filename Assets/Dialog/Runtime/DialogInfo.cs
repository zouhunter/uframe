//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-10-29 10:11:16
//* 描    述：  

//* ************************************************************************************

namespace UFrame.Dialog
{
    [System.Serializable]
    public class DialogInfo
    {
        public int id;
        public int uid;//角色id
        public int state;//立绘动画
        public string text;//文本信息
        public DialogOption[] options;//选项(按一定格式组合)
        public int nextId;//默认下一个对话id
    }
}