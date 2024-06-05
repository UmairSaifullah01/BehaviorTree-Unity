using UnityEngine;
using UnityEngine.Serialization;


namespace THEBADDEST.BT
{


	
	public sealed class ActionNode : Node
	{

		public string methodName;

		NodeStateDelegate action;

		public override void SetupNode(object treeRunner)
		{
			base.SetupNode(treeRunner);
			action = ReflectionHelper.CreateMethodInvoker(methodName, treeRunner);
		}

		public override NodeState Execute()
		{
			if (action != null)
				nodeState = action.Invoke();
			return nodeState;
		}

	}


}