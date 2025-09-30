//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-10-29 10:11:16
//* 描    述： 会话控制接口

//* ************************************************************************************
using System;

namespace UFrame.Dialog
{
    public interface IDialogCtrl
    {
        Action<DialogInfo> onShowDialog { get; set; }
        Action<DialogOption> onApplyDialogOption { get; set; }
        Action onDialogFinish { get; set; }
        void RegistDialogs(DialogInfo[] dialogs);
        void ShowDialog(int dialogId);
        void ApplyDialog(int optionIndex = 0);
    }
}
