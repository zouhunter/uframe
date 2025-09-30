/*-*-* Copyright (c) uframe@zht
  * Author: zouhunter
  * Creation Date: 2024-03-12
  * Version: 1.0.0
  * Description: 分层任务网路执行脚本
  *_*/
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UFrame.BehaviourTree;

namespace UFrame.HTN
{
    public abstract class NTreeRunnerBase : MonoBehaviour
    {
        [SerializeField]
        protected bool _continueRunning;
        [SerializeField]
        protected bool _autoStartOnEnable;
        [SerializeField]
        protected float _interval = 0.1f;
        protected float _intervalTimer;
        [SerializeField]
        protected NTree _nTree;
        [SerializeField]
        protected bool _openLog;
        [SerializeField]
        protected List<BindingInfo> _bindings;
        [SerializeField]
        public PreloadStates _preloadStates;
        public UnityEvent<NTree> onCreateBTreeEvent;
        protected bool _isRunning;
        public NTree TreeInstance => _instanceTree;
        protected NTree _instanceTree;
        protected abstract NTree CreateInstanceTree();

        [ContextMenu("OpenGraph")]
        public void OpenInstance()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.OpenAsset(_instanceTree ?? _nTree);
#endif
        }
        protected virtual void OnEnable()
        {
            _instanceTree = CreateInstanceTree();
            _instanceTree.LogInfo = _openLog;
            onCreateBTreeEvent?.Invoke(_instanceTree);
            if (_autoStartOnEnable)
            {
                foreach (var binding in _bindings)
                {
                    _instanceTree.SetVariable(binding.name, new Variable<UnityEngine.Object>() { Value = binding.target });
                }
                _isRunning = _instanceTree.StartUp();
                _preloadStates.CopyTo(_instanceTree.worldState);
            }
        }

        protected virtual void OnDisable()
        {
            if (_autoStartOnEnable)
            {
                _isRunning = false;
                _instanceTree.Stop();
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

            var result = _instanceTree.Tick();
            if (result == Status.Success || result == Status.Failure)
            {
                _isRunning = _continueRunning;
            }
        }
        protected virtual void OnDestroy()
        {
            if (_instanceTree != _nTree && _instanceTree)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    DestroyImmediate(_instanceTree);
                }
                else
#endif
                {
                    Destroy(_instanceTree);
                }
            }
        }
    }
}



