using System;
using UnityEngine;

namespace UFrame.HTN
{
	/// <summary>
	/// Effects have a string key and value of any type. An effect can be expected which means that
	/// the planner wont apply it on plan execution, but it will be applied in the search state.
	/// </summary>
	[Serializable]
	public class EffectInfo
	{
		/// <summary>
		/// Key for lookups.
		/// </summary>
		public string key;

		/// <summary>
		/// Expected effects are not applied in plan execution - only in search.
		/// </summary>
		public EffectApply apply;

		/// <summary>
		/// 效果类型
		/// </summary>
		public EffectType effectType;

		public virtual EffectType[] supportTypes { get; } = new EffectType[] { EffectType.Set, EffectType.Add, EffectType.Mult };

		/// <summary>
		/// Empty constructor for serialization.
		/// </summary>
		public EffectInfo()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public EffectInfo(string key, EffectApply apply = EffectApply.All)
		{
			this.key = key;
			this.apply = apply;
		}

		/// <summary>
		/// Apply effects to a blackboard.
		/// </summary>
		public virtual bool Apply(IWorldStates blackboard)
		{
			return false;
		}
	}

	[Serializable]
	public class BoolEffect : EffectInfo
	{
		public bool value;
		public override EffectType[] supportTypes { get; } = new EffectType[] { EffectType.Set };
		public override bool Apply(IWorldStates blackboard)
		{
			switch (effectType)
			{
				case EffectType.Set:
					blackboard.SetValue(key, value);
					break;
			}
			return true;
		}
	}

	[Serializable]
	public class IntEffect : EffectInfo
	{
		public int value;
		public override EffectType[] supportTypes { get; } = new EffectType[] { EffectType.Set, EffectType.Add, EffectType.Mult };
		public override bool Apply(IWorldStates blackboard)
		{
			switch (effectType)
			{
				case EffectType.Set:
					blackboard.SetValue(key, value);
					break;
				case EffectType.Add:
					if (blackboard.TryGetValue<int>(key, out int currentValue))
						blackboard.SetValue(key, currentValue + value);
					else
						blackboard.SetValue(key, value);
					break;
				case EffectType.Mult:
					if (blackboard.TryGetValue<int>(key, out int currentValue2))
						blackboard.SetValue(key, currentValue2 * value);
					else
						blackboard.SetValue(key, value);
					break;
			}
			return true;
		}
	}

	[Serializable]
	public class FloatEffect : EffectInfo
	{
		public float value;
		public override EffectType[] supportTypes { get; } = new EffectType[] { EffectType.Set, EffectType.Add, EffectType.Mult };
		public override bool Apply(IWorldStates blackboard)
		{
			switch (effectType)
			{
				case EffectType.Set:
					blackboard.SetValue(key, value);
					break;
				case EffectType.Add:
					if (blackboard.TryGetValue<float>(key, out float currentValue))
						blackboard.SetValue(key, currentValue + value);
					else
						blackboard.SetValue(key, value);
					break;
				case EffectType.Mult:
					if (blackboard.TryGetValue<float>(key, out float currentValue2))
						blackboard.SetValue(key, currentValue2 * value);
					else
						blackboard.SetValue(key, value);
					break;
			}
			return true;
		}
	}

	[Serializable]
	public class StringEffect : EffectInfo
	{
		public string value;
		public override EffectType[] supportTypes { get; } = new EffectType[] { EffectType.Set, EffectType.Add, EffectType.Mult };
		public override bool Apply(IWorldStates blackboard)
		{
			switch (effectType)
			{
				case EffectType.Set:
					blackboard.SetValue(key, value);
					break;
				case EffectType.Add:
					if (blackboard.TryGetValue<string>(key, out string currentValue))
						blackboard.SetValue(key, currentValue + value);
					else
						blackboard.SetValue(key, value);
					break;
				case EffectType.Mult:
					int n = 1;
					if (int.TryParse(value, out n) && n > 1 && blackboard.TryGetValue<string>(key, out string strVal))
						blackboard.SetValue(key, string.Concat(System.Linq.Enumerable.Repeat(strVal, n)));
					else
						blackboard.SetValue(key, value);
					break;
			}
			return true;
		}
	}

	[Serializable]
	public class GameObjectEffect : EffectInfo
	{
		public GameObject value;
		public override EffectType[] supportTypes { get; } = new EffectType[] { EffectType.Set };
		public override bool Apply(IWorldStates blackboard)
		{
			switch (effectType)
			{
				case EffectType.Set:
				case EffectType.Add:
				case EffectType.Mult:
					blackboard.SetValue(key, value);
					break;
			}
			return true;
		}
	}

	[Serializable]
	public class ObjectEffect : EffectInfo
	{
		public UnityEngine.Object value;
		public override EffectType[] supportTypes { get; } = new EffectType[] { EffectType.Set };
		public override bool Apply(IWorldStates blackboard)
		{
			switch (effectType)
			{
				case EffectType.Set:
				case EffectType.Add:
				case EffectType.Mult:
					blackboard.SetValue(key, value);
					break;
			}
			return true;
		}
	}

	[Serializable]
	public class NullableEffect : EffectInfo
	{
		public object value;
		public override EffectType[] supportTypes { get; } = new EffectType[] { EffectType.Set };
		public override bool Apply(IWorldStates blackboard)
		{
			switch (effectType)
			{
				case EffectType.Set:
				case EffectType.Add:
				case EffectType.Mult:
					blackboard.SetValue(key, value);
					break;
			}
			return true;
		}
	}
}
