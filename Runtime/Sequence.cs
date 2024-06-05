using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.BT
{


	
	public sealed class Sequence : Node
	{

		public List<Node> children = new List<Node>();

		public override NodeState Execute()
		{
			bool anyChildIsRunning = false;

			foreach (Node node in children)
			{
				switch (node.Execute())
				{
					case NodeState.FAILURE:
						nodeState = NodeState.FAILURE;
						return nodeState;
					case NodeState.SUCCESS:
						continue;
					case NodeState.RUNNING:
						anyChildIsRunning = true;
						continue;
					default:
						nodeState = NodeState.SUCCESS;
						return nodeState;
				}
			}

			nodeState = anyChildIsRunning ? NodeState.RUNNING : NodeState.SUCCESS;
			return nodeState;
		}

		public void AddChild(Node node)
		{
			children.Add(node);
		}

	}


}