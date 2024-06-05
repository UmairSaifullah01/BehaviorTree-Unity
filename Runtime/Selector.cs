using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.BT
{


	
	public sealed class Selector : Node
	{

		public List<Node> children = new List<Node>();

		public override NodeState Execute()
		{
			foreach (Node node in children)
			{
				switch (node.Execute())
				{
					case NodeState.FAILURE:
						continue;
					case NodeState.SUCCESS:
						nodeState = NodeState.SUCCESS;
						return nodeState;
					case NodeState.RUNNING:
						nodeState = NodeState.RUNNING;
						return nodeState;
					default:
						continue;
				}
			}

			nodeState = NodeState.FAILURE;
			return nodeState;
		}

		public void AddChild(Node node)
		{
			children.Add(node);
		}

	}


}