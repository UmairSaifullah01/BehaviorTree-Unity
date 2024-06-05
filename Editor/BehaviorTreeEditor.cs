using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace THEBADDEST.BT.Editor
{


	public class BehaviorTreeEditor : EditorWindow
	{

		private BehaviorTree tree;
		private Vector2      offset;
		private Vector2      drag;
		private Node         selectedNode;

		private const float gridSpacing   = 20f;
		private const float gridOpacity   = 0.2f;
		private const float gridThickness = 1f;

		private Texture2D gridTexture;
		private Vector2   gridTextureOffset;
		private Vector2   difference = new Vector2();
		GUIStyle          buttonStyle;
		string[]          methodNames;

		[MenuItem("Window/THEBADDEST/Behavior Tree Editor")]
		private static void OpenWindow()
		{
			BehaviorTreeEditor window = GetWindow<BehaviorTreeEditor>();
			window.titleContent = new GUIContent("Behavior Tree Editor");
			window.Show();
		}

		private void OnEnable()
		{
			gridTexture = new Texture2D(1, 1);
			gridTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, gridOpacity));
			gridTexture.Apply();
		}

		private void OnGUI()
		{
			SetupButtonStyle();
			DrawGridBackground();
			DrawToolbar();
			if (tree != null)
			{
				DrawTargetClassType();
				DrawNodes();
				DrawConnections();
				ProcessNodeEvents(Event.current);
				ProcessEvents(Event.current);
			}

			if (GUI.changed)
			{
				Repaint();
			}
		}

		void DrawTargetClassType()
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
			tree.defaultRunnerType = EditorGUILayout.TextField("", tree.defaultRunnerType);
			if (GUILayout.Button("Update"))
			{
				UpdateMethodNames();
			}

			EditorGUILayout.EndHorizontal();
		}

		void UpdateMethodNames()
		{
			Type type             = null;
			var  inheritedClasses = ReflectionHelper.GetInheritedClasses(typeof(BehaviorTreeMonoRunner));
			for (int i = 0; i < inheritedClasses.Length; i++)
			{
				Type inheritedClass = inheritedClasses[i];
				if (!inheritedClass.Name.Equals(tree.defaultRunnerType)) continue;
				type = inheritedClass;
				break;
			}

			if (type != null)
			{
				var methodInfos = ReflectionHelper.GetMethodsOfClassWithReturnTypeNodeState(type);
				if (methodInfos != null)
				{
					methodNames = new string[methodInfos.Length];
					for (int i = 0; i < methodInfos.Length; i++)
					{
						methodNames[i] = methodInfos[i].Name;
					}
				}
			}
			else
			{
				methodNames = null;
			}
		}

		void SetupButtonStyle()
		{
			if (buttonStyle == null)
			{
				buttonStyle           = new GUIStyle(GUI.skin.button);
				buttonStyle.alignment = TextAnchor.MiddleCenter; // Center align text
				// buttonStyle.fontSize  = 14;                      // Set font size
				// buttonStyle.fontStyle = FontStyle.Bold;
			}
		}

		private void DrawGridBackground()
		{
			float   width  = position.width;
			float   height = position.height;
			Vector2 offset = new Vector2(-gridTextureOffset.x % gridSpacing, -gridTextureOffset.y % gridSpacing);
			for (float x = offset.x; x < width; x += gridSpacing)
			{
				GUI.DrawTexture(new Rect(x, 0, gridThickness, height), gridTexture);
			}

			for (float y = offset.y; y < height; y += gridSpacing)
			{
				GUI.DrawTexture(new Rect(0, y, width, gridThickness), gridTexture);
			}
		}

		private void DrawToolbar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			tree = (BehaviorTree)EditorGUILayout.ObjectField("Behavior Tree", tree, typeof(BehaviorTree), false);
			if (GUILayout.Button("Create Root Node", EditorStyles.toolbarButton, GUILayout.Width(100)))
			{
				if (tree != null && tree.rootNode == null)
				{
					tree.rootNode      = CreateInstance<Selector>();
					tree.rootNode.name = "RootNode";
					AssetDatabase.AddObjectToAsset(tree.rootNode, tree);
					AssetDatabase.SaveAssets();
				}
			}

			GUILayout.EndHorizontal();
		}

		private void DrawNodes()
		{
			if (tree.rootNode != null)
			{
				DrawNode(tree.rootNode);
			}
		}

		private void DrawNode(Node node)
		{
			GUIStyle style = new GUIStyle { normal = { background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D }, border = new RectOffset(12, 12, 12, 12) };
			GUILayout.BeginArea(new Rect(node.position.x, node.position.y, 224, 74), style);
			GUIStyle normalStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
			buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
			EditorGUILayout.BeginVertical(normalStyle);
			if (node is Selector)
			{
				EditorGUILayout.LabelField(tree.rootNode == node ? "Root" : "Selector", normalStyle, GUILayout.Height(35));
				if (GUILayout.Button("Add Child", buttonStyle))
				{
					ProcessContextMenu(node.position + new Vector2(210, 0), node);
				}
			}
			else if (node is Sequence)
			{
				EditorGUILayout.LabelField("Sequence", normalStyle, GUILayout.Height(35));
				if (GUILayout.Button("Add Child", buttonStyle))
				{
					ProcessContextMenu(node.position + new Vector2(210, 0), node);
				}
			}
			else if (node is ConditionNode conditionNode)
			{
				EditorGUILayout.LabelField("Condition", normalStyle, GUILayout.Height(35));
				if (methodNames != null)
				{
					int indexOf = 0;
					if (!string.IsNullOrEmpty(conditionNode.methodName))
						indexOf = Array.IndexOf(methodNames, conditionNode.methodName);
					indexOf                  = EditorGUILayout.Popup(indexOf, methodNames);
					conditionNode.methodName = methodNames[indexOf];
				}
				else
				{
					conditionNode.methodName = EditorGUILayout.TextField("", conditionNode.methodName);
				}

				node.name = $"Condition {conditionNode.methodName}";
			}
			else if (node is ActionNode actionNode)
			{
				EditorGUILayout.LabelField("Action", normalStyle, GUILayout.Height(35));
				if (methodNames != null)
				{
					int indexOf = 0;
					if (!string.IsNullOrEmpty(actionNode.methodName))
						indexOf = Array.IndexOf(methodNames, actionNode.methodName);
					indexOf               = EditorGUILayout.Popup(indexOf, methodNames);
					actionNode.methodName = methodNames[indexOf];
				}
				else
				{
					actionNode.methodName = EditorGUILayout.TextField("", actionNode.methodName);
				}

				node.name = $"Action {actionNode.methodName}";
			}

			if (Application.isPlaying)
			{
				// Draw the light blob
				Rect lightBlobRect = new Rect(200, 10, 10, 10); // Position at top-right corner
				EditorGUI.DrawRect(lightBlobRect, node.nodeState != NodeState.FAILURE ? Color.green : Color.red);
			}

			EditorGUILayout.EndVertical();
			GUILayout.EndArea();

			// Draw children nodes
			if (node is Selector selectorNode)
			{
				foreach (var child in selectorNode.children)
				{
					DrawNode(child);
				}
			}
			else if (node is Sequence sequenceNode)
			{
				foreach (var child in sequenceNode.children)
				{
					DrawNode(child);
				}
			}
		}

		// private void DrawNode(Node node)
		// {
		// 	GUIStyle style = new GUIStyle { normal = { background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D }, border = new RectOffset(12, 12, 12, 12) };
		// 	GUILayout.BeginArea(new Rect(node.position.x, node.position.y, 224, 74), style);
		// 	GUIStyle normalStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
		// 	buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
		// 	EditorGUILayout.BeginVertical(normalStyle);
		// 	if (node is Selector)
		// 	{
		// 		EditorGUILayout.LabelField("Selector", normalStyle, GUILayout.Height(35));
		// 		if (GUILayout.Button("Add Child", buttonStyle))
		// 		{
		// 			ProcessContextMenu(node.position + new Vector2(210, 0), node);
		// 		}
		// 	}
		// 	else if (node is Sequence)
		// 	{
		// 		EditorGUILayout.LabelField("Sequence", normalStyle, GUILayout.Height(35));
		// 		if (GUILayout.Button("Add Child", buttonStyle))
		// 		{
		// 			ProcessContextMenu(node.position + new Vector2(210, 0), node);
		// 		}
		// 	}
		// 	else if (node is ConditionNode conditionNode)
		// 	{
		// 		EditorGUILayout.LabelField("Condition", normalStyle, GUILayout.Height(35));
		// 		if (methodNames != null && !string.IsNullOrEmpty(conditionNode.methodName))
		// 		{
		// 			int indexOf = Array.IndexOf(methodNames, conditionNode.methodName);
		// 			indexOf                  = EditorGUILayout.Popup(indexOf, methodNames);
		// 			conditionNode.methodName = methodNames[indexOf];
		// 		}
		// 		else
		// 		{
		// 			conditionNode.methodName = EditorGUILayout.TextField("", conditionNode.methodName);
		// 		}
		//
		// 		node.name = $"Condition {conditionNode.methodName}";
		// 	}
		// 	else if (node is ActionNode actionNode)
		// 	{
		// 		EditorGUILayout.LabelField("Action", normalStyle, GUILayout.Height(35));
		// 		if (methodNames != null && !string.IsNullOrEmpty(actionNode.methodName))
		// 		{
		// 			int indexOf = Array.IndexOf(methodNames, actionNode.methodName);
		// 			indexOf               = EditorGUILayout.Popup(indexOf, methodNames);
		// 			actionNode.methodName = methodNames[indexOf];
		// 		}
		// 		else
		// 		{
		// 			actionNode.methodName = EditorGUILayout.TextField("", actionNode.methodName);
		// 		}
		//
		// 		node.name = $"Action {actionNode.methodName}";
		// 	}
		//
		// 	EditorGUILayout.EndHorizontal();
		// 	GUILayout.EndArea();
		//
		// 	// Draw children nodes
		// 	if (node is Selector selectorNode)
		// 	{
		// 		foreach (var child in selectorNode.children)
		// 		{
		// 			DrawNode(child);
		// 		}
		// 	}
		// 	else if (node is Sequence sequenceNode)
		// 	{
		// 		foreach (var child in sequenceNode.children)
		// 		{
		// 			DrawNode(child);
		// 		}
		// 	}
		// }

		private void DrawConnections()
		{
			if (tree.rootNode != null)
			{
				DrawConnection(tree.rootNode);
			}
		}

		private void DrawConnection(Node node)
		{
			if (node is Selector selectorNode)
			{
				foreach (var child in selectorNode.children)
				{
					DrawNodeConnection(node, child);
					DrawConnection(child);
				}
			}
			else if (node is Sequence sequenceNode)
			{
				foreach (var child in sequenceNode.children)
				{
					DrawNodeConnection(node, child);
					DrawConnection(child);
				}
			}
		}

		private void DrawNodeConnection(Node parent, Node child)
		{
			Handles.DrawBezier(new Vector3(parent.position.x + 112, parent.position.y + 74, 0), new Vector3(child.position.x + 112, child.position.y, 0), new Vector3(parent.position.x + 112, parent.position.y + 112, 0), new Vector3(child.position.x + 112, child.position.y - 74, 0), Color.white, null, 2f);
		}

		private void ProcessEvents(Event e)
		{
			if (e.type != EventType.MouseDrag) return;
			if (e.button == 0 && selectedNode == null)
			{
				OnDrag(e.delta);
			}
		}

		private void ProcessNodeEvents(Event e)
		{
			if (tree.rootNode != null)
			{
				ProcessNodeEvent(tree.rootNode, e);
			}
		}

		private void ProcessNodeEvent(Node node, Event e)
		{
			if (node is Selector selectorNode)
			{
				foreach (var child in selectorNode.children)
				{
					ProcessNodeEvent(child, e);
				}
			}
			else if (node is Sequence sequenceNode)
			{
				foreach (var child in sequenceNode.children)
				{
					ProcessNodeEvent(child, e);
				}
			}

			Vector2 area = new Vector2(200, 50);
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						if (new Rect(node.position, area).Contains(e.mousePosition))
						{
							selectedNode = node;
							difference   = node.position + area / 2 - e.mousePosition;
							GUI.changed  = true;
						}
					}

					if (e.button == 1)
					{
						if (new Rect(node.position, area).Contains(e.mousePosition))
						{
							ProcessContextMenuForDelete(node.position + new Vector2(210, 0), node);
						}

						GUI.changed = true;
					}

					break;

				case EventType.MouseDrag:
					if (e.button == 0 && selectedNode != null)
					{
						selectedNode.position = e.mousePosition - area / 2 + difference;
						GUI.changed           = true;
					}

					break;

				case EventType.MouseUp:
					selectedNode = null;
					break;
			}
		}

		private void ProcessContextMenu(Vector2 mousePosition, Node parentNode)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Selector"),  false, () => OnAddNode(mousePosition, typeof(Selector),      parentNode));
			genericMenu.AddItem(new GUIContent("Add Sequence"),  false, () => OnAddNode(mousePosition, typeof(Sequence),      parentNode));
			genericMenu.AddItem(new GUIContent("Add Condition"), false, () => OnAddNode(mousePosition, typeof(ConditionNode), parentNode));
			genericMenu.AddItem(new GUIContent("Add Action"),    false, () => OnAddNode(mousePosition, typeof(ActionNode),    parentNode));
			genericMenu.ShowAsContext();
		}

		private void ProcessContextMenuForDelete(Vector2 mousePosition, Node parentNode)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Delete"), false, () => DeleteSelectedNode(mousePosition, parentNode));
			genericMenu.ShowAsContext();
		}

		private void OnAddNode(Vector2 mousePosition, System.Type type, Node parentNode)
		{
			Node newNode = CreateInstance(type) as Node;
			newNode.position = mousePosition;
			newNode.name     = type.Name;
			AssetDatabase.AddObjectToAsset(newNode, tree);
			AssetDatabase.SaveAssets();
			if (parentNode != null)
			{
				if (parentNode is Selector selectorNode)
				{
					selectorNode.AddChild(newNode);
				}
				else if (parentNode is Sequence sequenceNode)
				{
					sequenceNode.AddChild(newNode);
				}
			}
			else if (tree.rootNode == null)
			{
				tree.rootNode = newNode;
			}
		}

		private void DeleteSelectedNode(Vector2 mousePosition, Node deletedNode)
		{
			if (deletedNode != null)
			{
				RemoveNodeFromParent(tree.rootNode, deletedNode);
				DestroyImmediate(deletedNode, true);
				AssetDatabase.SaveAssets();
			}
		}

		private bool RemoveNodeFromParent(Node parent, Node nodeToRemove)
		{
			if (parent is Selector selectorNode)
			{
				if (selectorNode.children.Contains(nodeToRemove))
				{
					selectorNode.children.Remove(nodeToRemove);
					return true;
				}

				foreach (var child in selectorNode.children)
				{
					if (RemoveNodeFromParent(child, nodeToRemove))
					{
						return true;
					}
				}
			}
			else if (parent is Sequence sequenceNode)
			{
				if (sequenceNode.children.Contains(nodeToRemove))
				{
					sequenceNode.children.Remove(nodeToRemove);
					return true;
				}

				foreach (var child in sequenceNode.children)
				{
					if (RemoveNodeFromParent(child, nodeToRemove))
					{
						return true;
					}
				}
			}

			return false;
		}

		private void OnDrag(Vector2 delta)
		{
			drag              =  delta;
			gridTextureOffset += delta;
			if (tree.rootNode != null)
			{
				DragNode(tree.rootNode);
			}

			GUI.changed = true;
		}

		private void DragNode(Node node)
		{
			node.position += drag;
			if (node is Selector selectorNode)
			{
				foreach (var child in selectorNode.children)
				{
					DragNode(child);
				}
			}
			else if (node is Sequence sequenceNode)
			{
				foreach (var child in sequenceNode.children)
				{
					DragNode(child);
				}
			}
		}

	}


}