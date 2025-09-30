/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 动画播放模板                                                                    *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UFrame.Tween;

namespace UFrame.BridgeUI
{
    public abstract class AnimPlayer : ScriptableObject
    {
        [SerializeField]
        protected bool reverse;
        protected MonoBehaviour panel;
        protected UnityAction onComplete { get; set; }
        protected int completedTween;
        protected List<TweenBase> _tweeners;
        protected Coroutine coroutine;
        public virtual List<TweenBase> tweeners
        {
            get
            {
                if (_tweeners == null)
                {
                    _tweeners = CreateTweeners();
                }
                return _tweeners;
            }
        }
        public virtual void SetContext(MonoBehaviour context)
        {
            panel = context;
        }
        protected IEnumerator UpdateState()
        {
            var wait = new WaitForEndOfFrame();
            while (panel != null)
            {
                yield return wait;
                Update();
            }
        }

        protected virtual void Update()
        {
            if (tweeners != null)
            {
                foreach (var tween in tweeners)
                {
                    if (tween != null)
                    {
                        tween.Refresh();
                    }
                }
            }
        }

        public virtual void PlayAnim(UnityAction onComplete)
        {
            if (panel == null) return;

            if (coroutine != null)
            {
                panel.StopCoroutine(coroutine);
            }

            coroutine = panel.StartCoroutine(UpdateState());

            if (onComplete != null)
            {
                this.onComplete = onComplete;
            }

            if (tweeners != null)
            {
                completedTween = 0;

                foreach (var tween in tweeners)
                {
                    if (!reverse)
                    {
                        tween.PlayForward(true);
                    }
                    else
                    {
                        tween.PlayReverse(true);
                    }

                    tween.RegistOnFinished(CompleteOne);
                }
            }
        }
        
        public virtual AnimPlayer CreateInstance()
        {
            return Instantiate(this);
        }

        protected virtual void CompleteOne()
        {
            if (++completedTween >= tweeners.Count)
            {
                completedTween = 0;
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }
            }
        }

        protected abstract List<TweenBase> CreateTweeners();
    }
}
