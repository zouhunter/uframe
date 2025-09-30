using UnityEngine;

namespace UFrame.EasyBT.Tasks
{
    [AddComponentMenu("BehaviourTree/Decorate/ReferenceTask")]
    public class ReferenceTask : BaseNode
    {
        [SerializeField]
        private BaseNode _origin;
        protected override NodeStatus OnUpdate()
        {
            if(_origin == null)
                return NodeStatus.Inactive;
            return _origin.Status;
        }
    }
}
