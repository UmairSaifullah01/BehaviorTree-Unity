using System;
using System.Linq;
using System.Reflection;


namespace THEBADDEST.BT
{


	public static class ReflectionHelper
	{
		

		public static NodeStateDelegate CreateMethodInvoker(string methodName, object target)
		{
			if (string.IsNullOrEmpty(methodName))
			{
				throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));
			}

			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			// Get the type of the target object
			Type targetType = target.GetType();

			// Find the method on the target object
			MethodInfo methodInfo = targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodInfo == null)
			{
				throw new ArgumentException($"Method '{methodName}' not found on target of type '{targetType}'.");
			}

			// Create a delegate for the method
			NodeStateDelegate methodDelegate = (NodeStateDelegate)Delegate.CreateDelegate(typeof(NodeStateDelegate), target, methodInfo);
			return methodDelegate;
		}
		
		public static bool IsInheritedFromBehaviorTreeMonoRunner(Type targetType)
		{
			return targetType.IsSubclassOf(typeof(BehaviorTreeMonoRunner));
		}
		public static MethodInfo[] GetMethodsOfClassWithReturnTypeNodeState(Type targetType)
		{
			if (!IsInheritedFromBehaviorTreeMonoRunner(targetType))
				return null;
			
			return targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(method => method.ReturnType == typeof(NodeState))
				.ToArray();
		}
		public static Type[] GetInheritedClasses(Type baseType)
		{
			return Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(baseType))
				.ToArray();
		}

	}


}