/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 表控制器使用接口                                                                *
*//************************************************************************************/

namespace Jagat.TableCfg
{
    public interface ITableController
    {
        void SetLoader(ITextLoader tableLoader);
        void LoadTable(object tableId, string path,System.Type tableType, System.Action<object> onLoad = null);
        void LoadTable<T>(object tableId, string path,System.Action<Table<T>> onLoad = null) where T : IRow, new();
        bool CheckAllTableValied();
        Table<T> GetTable<T>(object tableId) where T : IRow, new();
        T GetConfig<T>(object tableId, int line) where T : IRow, new();
        void ClearAll();
    }
}