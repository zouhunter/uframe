/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 用户连线标记                                                                    *
*//************************************************************************************/

using System;

[AttributeUsage(AttributeTargets.Class)]
public class CustomConnectionAttribute : Attribute
{
    private string m_name;
    public string Name => m_name;

    public CustomConnectionAttribute(string name)
    {
        m_name = name;
    }
}
