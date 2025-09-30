//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CodeGroupAnim : CodeAnimBase
//{
//    [SerializeField]
//    protected List<CodeAnimBase> m_codeAnims;

//    public override bool IsPlaying => throw new System.NotImplementedException();

//    public override void Play()
//    {
//        if(m_codeAnims != null && m_codeAnims.Count > 0)
//        {
//            for (int i = 0; i < m_codeAnims.Count; i++)
//            {
//                if(m_codeAnims[i] != this)
//                    m_codeAnims[i].Play();
//            }
//        }
//    }

//    public override void ResetState()
//    {
//        if (m_codeAnims != null && m_codeAnims.Count > 0)
//        {
//            for (int i = 0; i < m_codeAnims.Count; i++)
//            {
//                if (m_codeAnims[i] != this)
//                    m_codeAnims[i].ResetState();
//            }
//        }
//    }

//    protected override void RecordState()
//    {
       
//    }
//}
