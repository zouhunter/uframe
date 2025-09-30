//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-10-29 10:11:16
//* 描    述： 会话控制器

//* ************************************************************************************
using System;
using System.Collections.Generic;

namespace UFrame.Dialog
{
    public class DialogCtrl : IDialogCtrl
    {
        public Action<DialogInfo> onShowDialog { get; set; }
        public Action<DialogOption> onApplyDialogOption { get; set; }
        public Action onDialogFinish { get; set; }
        protected DialogInfo m_currentDialog;
        protected Dictionary<int, DialogInfo> m_dialogMap = new Dictionary<int, DialogInfo>();

        public void ShowDialog(int dialogId)
        {
            if(m_dialogMap.TryGetValue(dialogId,out var dialog))
            {
                m_currentDialog = dialog;
                onShowDialog?.Invoke(dialog);
            }
            else
            {
                m_currentDialog = null;
                onDialogFinish?.Invoke();
            }
        }

        public void ApplyDialog(int index = 0)
        {
            if (m_currentDialog == null)
                return;

            if(m_currentDialog.options?.Length > index)
            {
                var dialogOption = m_currentDialog.options[index];
                onApplyDialogOption?.Invoke(dialogOption);
            }
            else
            {
                if(m_currentDialog.nextId > 0)
                {
                    ShowDialog(m_currentDialog.nextId);
                }
                else
                {
                    onDialogFinish?.Invoke();
                }
            }
        }

        public void RegistDialogs(DialogInfo[] dialogs)
        {
            for (int i = 0; i < dialogs.Length; i++)
            {
                var dialog = dialogs[i];
                if (dialog == null)
                    continue;

                m_dialogMap[dialog.id] = dialog;
            }
        }
    }
}
