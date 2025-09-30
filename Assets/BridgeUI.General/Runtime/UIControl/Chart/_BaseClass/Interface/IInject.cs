
using System.Collections.Generic;

namespace UFrame.BridgeUI.Chart
{
    public interface IInject
    {
        void Inject<T>(IList<T> data);
        void Inject<T>(IList<T>[] datas );
    }
}