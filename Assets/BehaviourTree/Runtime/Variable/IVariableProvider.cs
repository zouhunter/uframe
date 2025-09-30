using System;
using System.Collections.Generic;

namespace UFrame.BehaviourTree
{
    public interface IVariableProvider : IVariableContent
    {
        Variable<T> GetVariable<T>(string name, bool crateIfNotExits);
        Variable GetVariable(string name);
        void SetVariable(string key, Variable variable);
        void BindingExtraVariable(System.Func<string, Variable> variableGetter);
        Variable<T> GetVariable<T>(string name);
        bool TryGetVariable<T>(string name, out Variable<T> variable);
        bool TryGetVariable(string name, out Variable variable);
        void SetPersistentVariable(string variableName);
    }

    public interface IVariableContent
    {
        bool TryGetValue<T>(string name, out T value);
        T GetValue<T>(string name);
        bool TrySetValue(string key, object value);
        bool SetValue<T>(string key, T value);
        void CopyTo(IVariableContent target, List<string> keys = default);
    }
}
