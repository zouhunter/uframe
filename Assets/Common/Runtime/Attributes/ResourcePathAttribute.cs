//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************
using UnityEngine;
using System;

namespace UFrame
{
	public class ResourcePathAttribute : PropertyAttribute
	{

		public Type resourceType;

		public ResourcePathAttribute(Type t)
		{
			this.resourceType = t;
		}

	}
}