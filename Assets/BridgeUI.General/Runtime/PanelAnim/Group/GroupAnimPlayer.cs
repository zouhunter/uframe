using UnityEngine;
using System.Collections.Generic;
using UFrame.Tween;

namespace UFrame.BridgeUI
{
    public class GroupAnimPlayer : AnimPlayer
    {
        [SerializeField]
        protected List<AnimPlayer> childPlayers;
        public override void SetContext(MonoBehaviour context)
        {
            base.SetContext(context);
            foreach (var item in childPlayers)
            {
                item.SetContext(context);
            }
        }
        public override AnimPlayer CreateInstance()
        {
            GroupAnimPlayer instance = UnityEngine.Object.Instantiate(this);
            instance.childPlayers.Clear();
            for (int i = 0; i < childPlayers.Count; i++)
            {
                instance.childPlayers.Add(childPlayers[i].CreateInstance());
            }
            return instance;
        }
        protected override List<TweenBase> CreateTweeners()
        {
            var tweenList = new List<TweenBase>();
            foreach (var item in childPlayers)
            {
                tweenList.AddRange(item.tweeners);
            }
            return tweenList;
        }
    }
}