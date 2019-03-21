using UnityEngine;
using System.Collections.Generic;
using System;

public partial class MergeMgr {

    private const string ROOT_BONE_NAME = "Bip01"; // TODO: 根骨骼名
    private static MergeMgr _instance = null;
    public static MergeMgr Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MergeMgr();
            }
            return _instance;
        }
    }

    // 往go添加一套最大化骨架，返回go身上已添加的 Bip01 的骨架
    GameObject AddSkeletonFrame(GameObject go, UnityEngine.Object skeletonFrame)
    {
        GameObject sf = GameObject.Instantiate(skeletonFrame) as GameObject;
        // 寻找骨头根节点
        Transform rootBone = sf.transform.Find(ROOT_BONE_NAME);
        rootBone.SetParent(go.transform, false);
        GameObject.Destroy(sf);
        return rootBone.gameObject;
    }

	// 给骨架，部件，返回一个Go
    public GameObject DoMerge(UnityEngine.Object skeletonFrame, UnityEngine.Object[] partsPrefabs)
    {
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        List<Material> materials = new List<Material>();
        List<Transform> bones = new List<Transform>();

        GameObject result = GameObject.Instantiate(skeletonFrame) as GameObject;
        // 寻找骨头根节点
        GameObject rootBone = result.transform.Find(ROOT_BONE_NAME).gameObject;

        AddPartsData(rootBone, partsPrefabs, combineInstances, materials, bones);

        //添加mesh_root
        GameObject mesh = new GameObject("mesh_root");
        mesh.transform.position = Vector3.zero;
        mesh.transform.rotation = Quaternion.identity;
        mesh.transform.localScale = Vector3.one;
        mesh.transform.SetParent(result.transform, false);
        SkinnedMeshRenderer r = mesh.AddComponent<SkinnedMeshRenderer>();
        r.sharedMesh = new Mesh();
        r.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
        r.bones = bones.ToArray();
        r.rootBone = rootBone.transform;
#if UNITY_EDITOR
		if(!Application.isPlaying){
			r.sharedMaterials = materials.ToArray();
		}else{
			r.materials = materials.ToArray();
		}
#else
		r.materials = materials.ToArray();
#endif
        return result;
    }

	public GameObject DoMergeByGo(GameObject target, UnityEngine.Object[] partsPrefabs, bool isPartGo = false)
	{
		if(target == null){
			target = new GameObject();
		}
		
		// 寻找骨头根节点
        Transform rootBoneTrans = target.transform.Find(ROOT_BONE_NAME);
        if(null == rootBoneTrans){
            return target;
        }
		GameObject rootBone = rootBoneTrans.gameObject;

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        List<Material> materials = new List<Material>();
        List<Transform> bones = new List<Transform>();

		AddPartsData(rootBone, partsPrefabs, combineInstances, materials, bones, isPartGo);

		//添加mesh_root
		GameObject mesh = null;
		SkinnedMeshRenderer r = null;
		Transform meshT = target.transform.Find ("mesh_root");
		if (meshT != null) {
			mesh = meshT.gameObject;
			r = mesh.GetComponent<SkinnedMeshRenderer>();
		} else {
			mesh = new GameObject("mesh_root");
			mesh.transform.position = Vector3.zero;
			mesh.transform.rotation = Quaternion.identity;
			mesh.transform.localScale = Vector3.one;
			mesh.transform.SetParent(target.transform, false);
		}
		if(r == null){
			r = mesh.AddComponent<SkinnedMeshRenderer>();
		}
		r.sharedMesh = new Mesh();
		r.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
		r.bones = bones.ToArray();
        r.rootBone = rootBone.transform;
        //r.sharedMesh.bounds = new Bounds(Vector3.zero, r.sharedMesh.bounds.extents);
		#if UNITY_EDITOR
		if(!Application.isPlaying){
			r.sharedMaterials = materials.ToArray();
		}else{
			r.materials = materials.ToArray();
		}
		#else
		r.materials = materials.ToArray();
		#endif
		return target;
	}

	void AddPartsData(GameObject rootBone, UnityEngine.Object[] partsPrefabs, List<CombineInstance> coms, List<Material> mtls, List<Transform> bones, bool isPartGo = false)
    {
        Transform[] allBones = rootBone.GetComponentsInChildren<Transform>();
        Dictionary<string, Transform> allGuaMap = new Dictionary<string, Transform>();
        for (int i = 0; i < allBones.Length; i++)
        {
            Transform tran = allBones[i];
            if (!allGuaMap.ContainsKey(tran.name) && tran.name.StartsWith("gua_")){
                allGuaMap.Add(tran.name, tran);
            }
        }

        for (int i = 0; i < partsPrefabs.Length; i++)
        {
            UnityEngine.Object partPerfab = partsPrefabs[i];
			GameObject partObj = null;
			if (isPartGo) {
				partObj = partPerfab as GameObject;
			} else {
				partObj = (GameObject)GameObject.Instantiate (partPerfab);
			}
			SkinnedMeshRenderer smr = partObj.GetComponentInChildren<SkinnedMeshRenderer>();
			Debug.Assert(smr != null, string.Format("MergeSummonMgr: {0} 没有 SkinnedMeshRenderer", partPerfab.name));
			MergeMesh(coms, bones, smr, allBones);
			MergeMaterials(mtls, smr);
			#if UNITY_EDITOR
			if(!Application.isPlaying){
				GameObject.DestroyImmediate(partObj);
			}else{
				GameObject.Destroy(partObj);
			}
			#else
			GameObject.Destroy(partObj);
			#endif
        }
    }


    void MergeMaterials(List<Material> mats, SkinnedMeshRenderer srcMesh)
    {
#if UNITY_EDITOR
		if(!Application.isPlaying){
			mats.AddRange(srcMesh.sharedMaterials);
		}else{
			mats.AddRange(srcMesh.materials);
		}
#else
		mats.AddRange(srcMesh.materials);
#endif

    }

    void MergeMesh(List<CombineInstance> coms, List<Transform> bones, SkinnedMeshRenderer srcMesh, Transform[] allBones)
    {
        //Debug.Assert(srcMesh.sharedMesh != null, string.Format("{0} miss mesh", srcMesh.gameObject.name));
		if(null == srcMesh.sharedMesh){
			//Debugger.LogWarning (string.Format("MergeSummonMgr: {0} miss mesh", srcMesh.gameObject.name));
			return;
		}
        for (int sub = 0; sub < srcMesh.sharedMesh.subMeshCount; sub++)
        {
            CombineInstance ci = new CombineInstance();
            //if (srcMesh.sharedMesh == null) Debugger.Log(string.Format("{0} miss mesh", srcMesh.gameObject.name));
            ci.mesh = srcMesh.sharedMesh;
            ci.subMeshIndex = sub;
            coms.Add(ci);
            MergeBones(bones, srcMesh, allBones);
        }
    }

    // 注意,避免骨头名字重复
    void MergeBones(List<Transform> bones, SkinnedMeshRenderer srcMesh, Transform[] allBones)
    {
        Transform[] srcBones = srcMesh.bones;
        foreach (Transform boneS in srcBones)
        {
            foreach (Transform boneM in allBones)
            {
                if (boneS != null && boneM != null)
                {
                    if (boneS.name != boneM.name)
                        continue;
                }
                bones.Add(boneM);
            }
        }
    }
}
