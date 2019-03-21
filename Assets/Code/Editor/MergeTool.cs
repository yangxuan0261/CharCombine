using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Qtz
{
    public class MergeTool
    {
		[MenuItem("Assets/角色合成")]
		public static void Merge()
		{
			UnityEngine.Object[] selObjs = Selection.GetFiltered(typeof(UnityEngine.GameObject), SelectionMode.Assets);
			if (selObjs == null || selObjs.Length == 0)
			{
				Debug.LogErrorFormat("请选择需要合成的玩家的主骨架prefab!");
				return;
			}
			if (selObjs == null || selObjs.Length == 1)
			{
				Debug.LogErrorFormat("请选择需要合成的玩家的部件prefab!");
				return;
			}

			//TODO: 取名字最短的作为主骨架
			int objNameLen = selObjs[0].name.Length;
			UnityEngine.Object mainObj = selObjs[0];
			UnityEngine.Object[] partObjs = new Object[selObjs.Length - 1];
			for (int i = 0; i < selObjs.Length; ++i)
			{
				if(selObjs[i].name.Length < objNameLen){
					objNameLen = selObjs [i].name.Length;
					mainObj = selObjs [i];
				}
			}
			int j = 0;
			for (int i = 0; i < selObjs.Length; ++i)
			{
				if(selObjs [i] != mainObj){
					partObjs [j] = selObjs [i];
					j++;
				}
			}
			GameObject go = GameObject.Instantiate(mainObj) as GameObject;
			MergePart mergePart = go.GetComponent<MergePart>();
			if(mergePart == null){
				mergePart = go.AddComponent<MergePart>();
			}
			mergePart.mainPart = mainObj;
			mergePart.subParts = new List<UnityEngine.Object>(partObjs);
			mergePart.DoMerge();
		}
    }
}

