/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 行为树行为脚本
 *_*/

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.BehaviourTree
{
    public abstract class BTreeRunnerBase : MonoBehaviour
    {
        [SerializeField]
        protected bool _continueRunning;
        [SerializeField]
        protected bool _autoStartOnEnable;
        [SerializeField]
        protected float _interval = 0.1f;
        protected float _intervalTimer;
        [SerializeField]
        protected BTree _btInstance;
        [SerializeField]
        protected bool _openLog;
        [SerializeField]
        protected List<BindingInfo> _bindings;
        public UnityEvent<BTree> onCreateBTreeEvent;
        protected bool _isRunning;
        public BTree BTreeInstance => _btInstance;
        protected BTree _createdTree;
        protected abstract BTree CreateInstanceTree();
        protected virtual void OnEnable()
        {
            if (!_btInstance)
            {
                _createdTree = CreateInstanceTree();
                _btInstance = _createdTree;
                _btInstance.LogInfo = _openLog;
                onCreateBTreeEvent?.Invoke(_btInstance);
            }
            if (_autoStartOnEnable)
            {
                foreach (var binding in _bindings)
                {
                    _btInstance.SetVariable(binding.name, new Variable<UnityEngine.Object>() { Value = binding.target });
                }
                _isRunning = _btInstance.StartUp();
            }
        }

        protected virtual void OnDisable()
        {
            if (_autoStartOnEnable)
            {
                _isRunning = false;
                _btInstance.Stop();
            }
        }

        protected virtual void Update()
        {
            if (!_isRunning)
                return;

            if (_interval > 0 && _intervalTimer < _interval)
            {
                _intervalTimer += Time.deltaTime;
                return;
            }
            _intervalTimer = 0;

            var result = _btInstance.Tick();
            if (result == Status.Success || result == Status.Failure)
            {
                _isRunning = _continueRunning;
            }
        }
        protected virtual void OnDestroy()
        {
            if (_createdTree)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    DestroyImmediate(_createdTree);
                }
                else
#endif
                {
                    Destroy(_createdTree);
                }
            }
        }
    }
}

