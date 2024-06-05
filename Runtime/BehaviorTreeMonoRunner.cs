using UnityEngine;


namespace THEBADDEST.BT
{


	public abstract class BehaviorTreeMonoRunner : MonoBehaviour
	{

		[SerializeField] protected BehaviorTree behaviorTree;
	
		public virtual void SetupTree()
		{
			if(behaviorTree==null) Debug.Log("No Behavior Tree Assigned");
			behaviorTree.Setup(this);
		}
		
		public virtual void Execute()
		{
			if (behaviorTree != null)
			{
				behaviorTree.Execute();
			}
		}


		
		
	}


}