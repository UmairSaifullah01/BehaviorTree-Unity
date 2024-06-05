using UnityEngine;
using UnityEngine.Serialization;


namespace THEBADDEST.BT
{


	public sealed class ConditionNode : Node
	{

		public string     methodName;
		NodeStateDelegate condition;

		public override void SetupNode(object treeRunner)
		{
			base.SetupNode(treeRunner);
			condition = ReflectionHelper.CreateMethodInvoker(methodName, treeRunner);
		}

		public override NodeState Execute()
		{
			if (condition != null)
				nodeState = condition.Invoke();
			return nodeState;
		}

	}


}