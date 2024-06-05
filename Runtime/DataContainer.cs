using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.BT
{


	public abstract class DataContainer : ScriptableObject
	{

		private Dictionary<string, object> dataContext = new Dictionary<string, object>();

		protected abstract DataContainer parent { get; set; }

		public void SetData(string key, object value)
		{
			dataContext[key] = value;
		}

		public object GetData(string key)
		{
			object value = null;
			if (dataContext.TryGetValue(key, out value))
				return value;
			DataContainer node = parent;
			while (node != null)
			{
				value = node.GetData(key);
				if (value != null)
					return value;
				node = node.parent;
			}

			return null;
		}

		public bool ClearData(string key)
		{
			if (dataContext.ContainsKey(key))
			{
				dataContext.Remove(key);
				return true;
			}

			DataContainer node = parent;
			while (node != null)
			{
				bool cleared = node.ClearData(key);
				if (cleared)
					return true;
				node = node.parent;
			}

			return false;
		}

	}


}