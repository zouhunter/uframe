using System.Collections.Generic;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    public class PreloadStates : ScriptableObject
    {
        [SerializeField]
        private List<Variable<string>> stringVariables = new();
        [SerializeField]
        private List<Variable<float>> floatVariables = new();
        [SerializeField]
        private List<Variable<int>> intVariables = new();
        [SerializeField]
        private List<Variable<bool>> boolVariables = new();
        [SerializeField]
        private List<Variable<Vector3>> vector3Variables = new();

        public virtual void CopyTo(IVariableContent variableContent)
        {
            foreach (var item in stringVariables)
            {
                SetVariable(item.Name, item, variableContent);
            }

            foreach (var item in floatVariables)
            {
                SetVariable(item.Name, item, variableContent);
            }

            foreach (var item in intVariables)
            {
                SetVariable(item.Name, item, variableContent);
            }

            foreach (var item in boolVariables)
            {
                SetVariable(item.Name, item, variableContent);
            }

            foreach (var item in vector3Variables)
            {
                SetVariable(item.Name, item, variableContent);
            }
        }

        public void SetVariable<T>(string name, Variable<T> data, IVariableContent variableContent)
        {
            if (variableContent is IVariableProvider privoder)
            {
                privoder.SetVariable(name, data);
            }
            else
            {
                variableContent.SetValue(name, data.Value);
            }
        }
    }
}