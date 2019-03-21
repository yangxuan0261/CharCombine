
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MergePart : MonoBehaviour {
	public UnityEngine.Object mainPart = null;
	public List<UnityEngine.Object> subParts = new List<UnityEngine.Object>();

	public void DoMerge(){
		if(mainPart != null && subParts != null){
			UnityEngine.Object[] partObjs = subParts.ToArray();
			MergeMgr.Instance.DoMergeByGo(gameObject, partObjs);
		}
	}
}
