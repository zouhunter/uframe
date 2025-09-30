using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Tween
{
    public class TweenCtrl
    {
        public List<ITween> m_tweens;

        public TweenCtrl()
        {
            m_tweens = new List<ITween>();
        }

        public void Refresh(bool fixedUpdate)
        {
            for (int i = 0; i < m_tweens.Count; i++)
            {
                var tween = m_tweens[i];
                if(tween.IgnoreTimeScale != fixedUpdate)
                {
                    continue;
                }
                if (!tween.Valid || !tween.Enabled)
                {
                    m_tweens.RemoveAt(i);
                    i--;
                }
                else
                {
                    try
                    {
                        tween.Refresh();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                        tween.DisableTween();
                        m_tweens.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public bool StartTween(ITween tween)
        {
            if (tween != null && tween.Valid && !m_tweens.Contains(tween))
            {
                m_tweens.Add(tween);
                tween.EnableTween();
                return true;
            }
            return false;
        }

        public bool StopTween(ITween tween)
        {
            var ok = m_tweens.Remove(tween);
            if (ok)
            {
                tween.DisableTween();
                return true;
            }
            return false;
        }
    }
}