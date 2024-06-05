using UnityEngine;


namespace THEBADDEST.BT
{


	#if UNITY_EDITOR
	public partial class BehaviorTree : ScriptableObject
	{

		public string defaultRunnerType;

	}
	#endif


	[CreateAssetMenu(menuName = "THEBADDEST/BehaviorTree")]
	public partial class BehaviorTree : ScriptableObject
	{

		public Node rootNode;

		public void Setup(object target)
		{
			SetupNode(rootNode, target);
		}

		private void SetupNode(Node node, object target)
		{
			if (node is ConditionNode conditionNode)
			{
				conditionNode.SetupNode(target);
			}
			else if (node is ActionNode actionNode)
			{
				actionNode.SetupNode(target);
			}
			else if (node is Selector selectorNode)
			{
				foreach (var child in selectorNode.children)
				{
					SetupNode(child, target);
				}
			}
			else if (node is Sequence sequenceNode)
			{
				foreach (var child in sequenceNode.children)
				{
					SetupNode(child, target);
				}
			}
		}

		public void Execute()
		{
			if (rootNode != null)
			{
				rootNode.Execute();
			}
		}

	}


}