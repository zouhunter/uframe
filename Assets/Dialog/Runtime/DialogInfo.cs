//*************************************************************************************
//* ��    �ߣ� �޺���
//* ����ʱ�䣺 2021-10-29 10:11:16
//* ��    ����  

//* ************************************************************************************

namespace UFrame.Dialog
{
    [System.Serializable]
    public class DialogInfo
    {
        public int id;
        public int uid;//��ɫid
        public int state;//���涯��
        public string text;//�ı���Ϣ
        public DialogOption[] options;//ѡ��(��һ����ʽ���)
        public int nextId;//Ĭ����һ���Ի�id
    }
}