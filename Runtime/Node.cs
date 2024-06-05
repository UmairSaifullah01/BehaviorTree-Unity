using UnityEngine;
using UnityEngine;


namespace THEBADDEST.BT
{


	#if UNITY_EDITOR
	public abstract partial class Node : DataContainer
	{

		[HideInInspector]public Vector2 position;

	}
	#endif
	// for Editor Only


	public abstract partial class Node : DataContainer
	{

		public          NodeState     nodeState;
		protected override DataContainer parent { get; set; }

		public virtual void SetupNode(object treeRunner)
		{
			nodeState = NodeState.FAILURE;
		}

		protected virtual void Attach(Node node)
		{
			node.parent = this;
		}

		public abstract NodeState Execute();

	}


}