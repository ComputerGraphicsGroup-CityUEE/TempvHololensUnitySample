using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using q_common;
using System.Linq;
using Unity.VisualScripting;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        #region vulkan version native plugin
        [DllImport("Vulkan_temvecvis_gen", EntryPoint = "weld_func")]
        private unsafe static extern void weld_func(Vector3* _ls_v,                      // vpos 
                                                    int* _vtIdx,
                                                    int _dnv, int _nf,                   // _dnv: non-welded vertex number
                                                    int* _weld_vtIdx,                    // output, length = 1728
                                                    int* _weld_vidx_all,                 // output, length = 1728
                                                    int* _weld_vidx_allInOne,            // output, length = 1728
                                                    int* _weld_vidx_perSize,             // output, length = 1728
                                                    int* _weld_vSize);                   // output, length = 3

        [DllImport("Vulkan_temvecvis_gen", EntryPoint = "weld_func_ab")]
        private unsafe static extern void weld_func_ab(Vector3* _ls_v,                      // vpos 
                                                        int* _vtIdx,
                                                        int _dnv, int _nf,                   // _dnv: non-welded vertex number
                                                        int* _weld_vtIdx,                    // output, length = 1728
                                                        int* _weld_vidx_all,                 // output, length = 1728
                                                        int* _weld_vidx_allInOne,            // output, length = 1728
                                                        int* _weld_vidx_perSize,             // output, length = 1728
                                                        int* _weld_vidx_mapping,
                                                        int* _weld_vSize);                   // output, length = 3

        [DllImport("Vulkan_temvecvis_gen", EntryPoint = "cal_edge_func_ab")]
        private unsafe static extern void cal_edge_func_ab(int* weld_vtidx,                      // vtidx after welding
                                                            int* _weld_vidx_all,
                                                            int _nv, int _nf, int _weld_nv,      // _nv: welded vertex number = 290, _nf = #triangle
                                                            Vector4* _vfEdges,                   // output, fixed size = 1024*1024 = 1048576
                                                            int* _fEdges,                        // output, length = 1728
                                                            Vector4* _vfEdgesMap,                // output, fixed size = 1024*1024 = 1048576
                                                            int* _edgeSize);                     // output, length = 1

        [DllImport("Vulkan_temvecvis_gen", EntryPoint = "gen_tempVecVis_ai")]
        private unsafe static extern void gen_tempVecVis_ai(Vector3* _ls_v,                      // vpos 
                                                    Vector3* _ls_vn,                     // vnormal
                                                    int* _vtIdx,
                                                    int* _weld_vtIdx,
                                                    int* _weld_vidx_all,
                                                    Vector3* _tNormal,
                                                    Vector4* _vfEdges,
                                                    Vector4* _vfEdgesMap,
                                                    int* _fEdges,
                                                    int _nf, int _dnv,          // _nf = #triangle, _dnv: non-welded vertex number
                                                    int _nv, int _ne,           // _nv: welded vertex number = 290, _ne: _vfEdges.size
                                                    int* des_ei,                // output, fixed size = 1024*1024*nv = 304087040
                                                    float* des_es,              // output, fixed size = 1024*1024*nv = 304087040
                                                    int* des_vinfo0,            // output, length = nv+1
                                                    int* vis_size);             // output, length = 1    

        [DllImport("Vulkan_temvecvis_gen", EntryPoint = "gen_tempVecVis_ondemand_ac")]
        private unsafe static extern void gen_tempVecVis_ondemand_ac(Vector3* _ls_v,
                                                             Vector3* _ls_vn,
                                                             int* _vtIdx,
                                                             int* _weld_vtIdx,
                                                             int* _weld_vidx_all,
                                                             Vector3* _tNormal,
                                                             Vector4* _vfEdges,
                                                             Vector4* _vfEdgesMap,
                                                             int* _fEdges,
                                                             int _nf, int _dnv,          // _nf = #triangle, _dnv: non-welded vertex number
                                                             int _nv, int _ne,           // _nv: welded vertex number = 290, _ne: _vfEdges.size
                                                             int _first_vi,           // _nv: welded vertex number = 290, _ne: _vfEdges.size
                                                             int* des_ei,                // output, fixed size = 1024*1024*nv = 304087040
                                                             float* des_es,                // output, fixed size = 1024*1024*nv = 304087040
                                                             int* des_vinfo0,            // output, length = nv+1
                                                             int* vis_size);             // output, length = 1  

        [DllImport("Vulkan_temvecvis_gen", EntryPoint = "get_singleV_vecvis_ac")]
        private unsafe static extern void get_singleV_vecvis_ac(Vector3* _ls_v,             // vpos 
                                                                Vector3* _ls_vn,            // vnormal
                                                                int* _weld_vidx_all,        // 290
                                                                Vector4* _vfEdgesMap,       // 864
                                                                int* des_ei,                // 172838 
                                                                float* des_es,              // 172838
                                                                int* des_vinfo0,            // length = nv+1 = 291
                                                                int _nv, int _nf,           // _nv: welded vertex number = 290, _nf = #triangle = 576
                                                                int _nei, int _dnv,         // _nei = 172838,  _dnv: non-welded vertex number = 1728
                                                                int _neMap,                 // _ne: _vfEdges.size
                                                                int _vIdx,                  // current vertex idx of visibility
                                                                Vector3* des_vis_single,    // output: fixed length = 1024*1024
                                                                int* vis_single_size);      // output: fixed length = 1
        #endregion

        qObjectManager m_qObjectManager;
        GameObject q_obj;
        List<qVtAnimData> objAnimOSList;                    // Container of vertex animation data <qVtAnimData>
        List<qVtAnimData.qFrameData[]> frameDatasWS;        // Unitized, merge object vertex data in every frame in every animations 
        List<qVtAnimData.qFrameData[]> frameDatasWS_flipZ;  
        List<List<Vector3[]>> frameTNormalWS_flipZ;         // [animation][frame][tNormal]
        List<List<Vector3[]>> frameTNormalWS;               // Face normal calculated from unitized merge object 
        float _time;
        int currentf;
        int AnimIdx = 2;

        #region variable for dll
        List<Vector3> ls_v = new List<Vector3>();
        List<Vector3> ls_vn = new List<Vector3>();
        List<int> vtIdx = new List<int>();
        List<Vector3> tn = new List<Vector3>();

        // after welding vertext
        List<int> weld_vtIdx = new List<int>();
        List<int> weld_v_all = new List<int>();
        List<int> weld_v_allInOne = new List<int>();
        List<int> weld_v_perSize = new List<int>();
        List<int> weld_vtIdx_map = new List<int>();
        List<int> weld_vSize = new List<int>();

        // vulkan_calculate_edge_func_output
        List<Vector4> vfEdges = new List<Vector4>();
        List<Vector4> vfEdgesMap = new List<Vector4>();
        List<int> fEdges = new List<int>();
        List<int> edgeSize = new List<int>();

        // output container, containing with info passed from c++
        List<int> des_ei = new List<int>();
        List<float> des_es_float = new List<float>();
        List<int> des_vibility = new List<int>();
        List<int> des_vis_size = new List<int>();

        // output container, containing with single vertex evaluation info passed from c++
        List<Vector3> des_vis_single = new List<Vector3>();
        List<int> vis_single_size = new List<int>();
        #endregion

        bool isVisibilityNeedToBeBuilt;

        Vector3[] ls_v_arr;
        Vector3[] ls_vn_arr;
        int[] vtIdx_arr;
        int[] weld_vtIdx_arr;
        int[] weld_v_all_arr;
        Vector3[] tn_arr;
        Vector4[] vfEdges_arr;
        Vector4[] vfEdgesMap_arr;
        int[] fEdges_arr;

        int[] des_ei_arr;
        float[] des_es_arr;
        int[] des_vinfo_arr;
        int[] vis_size_arr;

        int planeVertexCount;

        public void InitTemporalVisibility()
        {
            Clear();
            currentf = -1;

            frameDatasWS = new();
            frameDatasWS_flipZ = new();
            frameTNormalWS = new();
            frameTNormalWS_flipZ = new();
            objAnimOSList = new List<qVtAnimData>();
            isVisibilityNeedToBeBuilt = false;

            q_obj = GameObject.Find("Merge Object");

            m_qObjectManager = new qObjectManager();
            m_qObjectManager.LoadObjVertexAnimation(ref SceneManager.Instance.tempVecObjs, ref objAnimOSList);
            m_qObjectManager.UnitizeObjects(ref SceneManager.Instance.tempVecObjs, ref q_obj, ref frameDatasWS, ref planeVertexCount, objAnimOSList);

            foreach (GameObject obj in SceneManager.Instance.tempVecObjs)
                obj.transform.hasChanged = false;

            ls_v = m_qObjectManager.vPos_unitize;
            ls_vn = m_qObjectManager.vNormals;
            vtIdx = m_qObjectManager.vtIdx;
            //q_common.q_IO.SaveVector3ArrayToFile(ls_v.ToArray(), "ls_v_plane");

            ls_v = q_common.q_common.FilpZAxis(ls_v);
            ls_vn = q_common.q_common.FilpZAxis(ls_vn);
            //Triangle normal need to be calculated here so that they can stay the same coordinate as ls_v easily.
            tn = q_common.q_common.GetFaceNormal(ls_v.ToArray(), vtIdx.ToArray(), m_qObjectManager.nTriangle).ToList();

            frameDatasWS_flipZ = new List<qVtAnimData.qFrameData[]>();
            for (int i = 0; i < frameDatasWS.Count; i++)
            {
                int frameLen = frameDatasWS[i].Length;
                List<Vector3[]> m_tNormalList = new();
                qVtAnimData.qFrameData[] temp_qFrameData_list = new qVtAnimData.qFrameData[frameLen];
                for (int k = 0; k < frameLen; k++)
                {
                    qVtAnimData.qFrameData temp_qFrameData = new qVtAnimData.qFrameData();

                    Vector3[] temp_animVertex = q_common.q_common.FilpZAxis(frameDatasWS[i][k].animVertex);
                    Vector3[] temp_animVNormal = q_common.q_common.FilpZAxis(frameDatasWS[i][k].animVNormal);
                    Vector3[] m_tn = q_common.q_common.GetFaceNormal(temp_animVertex, vtIdx.ToArray(), m_qObjectManager.nTriangle);

                    temp_qFrameData.animVertex = temp_animVertex;
                    temp_qFrameData.animVNormal = temp_animVNormal;
                    temp_qFrameData.time = frameDatasWS[i][k].time;

                    m_tNormalList.Add(m_tn);
                    temp_qFrameData_list[k] = temp_qFrameData;
                }
                frameDatasWS_flipZ.Add(temp_qFrameData_list);
                frameTNormalWS_flipZ.Add(m_tNormalList);
            }

            #region Get Temporal Visibility in DLL

            GetWeldVertex(ref weld_vtIdx, ref weld_v_all, ref weld_v_allInOne, ref weld_v_perSize, ref weld_vtIdx_map, ref weld_vSize);

            //Debug.Log(weld_vtIdx_map.Count);
            GetEdgeInfo(ref vfEdges, ref fEdges, ref vfEdgesMap, ref edgeSize);

            ls_v_arr = ls_v.ToArray();
            ls_vn_arr = ls_vn.ToArray();
            vtIdx_arr = vtIdx.ToArray();
            weld_vtIdx_arr = weld_vtIdx.ToArray();
            weld_v_all_arr = weld_v_all.ToArray();
            tn_arr = tn.ToArray();
            vfEdges_arr = vfEdges.ToArray();
            vfEdgesMap_arr = vfEdgesMap.ToArray();
            fEdges_arr = fEdges.ToArray();

            des_ei_arr = new int[1024 * 64];
            des_es_arr = new float[1024 * 64];
            des_vinfo_arr = new int[weld_v_all.Count + 1];
            vis_size_arr = new int[2];

            GetVisibility(0);
            //GetVisibility(ref des_ei, ref des_es_float, ref des_vibility, ref des_vis_size);

            #endregion
        }

        void RegenVisibility() 
        {
            if (SceneManager.isPlayAnimation == true)
            {
                // update vertex to model
                _time += Time.deltaTime;
                _time %= objAnimOSList[AnimIdx].animLen;
                int f = (int)(_time / (1.0f / objAnimOSList[AnimIdx].frame));
                Mesh m_sharedMesh = q_obj.GetComponent<MeshFilter>().sharedMesh;
                Mesh m_sharedMesh_2 = GameObject.Instantiate(m_sharedMesh);

                m_sharedMesh_2.vertices = frameDatasWS[AnimIdx][f].animVertex;
                m_sharedMesh_2.normals = frameDatasWS[AnimIdx][f].animVNormal;

                q_obj.GetComponent<MeshFilter>().sharedMesh = m_sharedMesh_2;

                //// update visibility
                ls_v_arr = frameDatasWS_flipZ[AnimIdx][f].animVertex;
                ls_vn_arr = frameDatasWS_flipZ[AnimIdx][f].animVNormal;
                tn_arr = frameTNormalWS_flipZ[AnimIdx][f];

                if (SceneManager.checkFrame % 8 == 0)
                    GetVisibility(0);
                else if (SceneManager.checkFrame % 2 == 0)
                    GetVisibility(weld_v_all_arr.Length - 121);
                

                isVisibilityNeedToBeBuilt = true;
                SceneManager.checkFrame++;
            }

            else if (SceneManager.isPlayAnimation == false)
            {
                // update vertex to model
                //_time += Time.deltaTime;
                _time %= objAnimOSList[AnimIdx].animLen;
                int f = (int)(_time / (1.0f / objAnimOSList[AnimIdx].frame));
                if (q_obj == null)
                    q_obj = GameObject.Find("Merge Object");

                Mesh m_sharedMesh = q_obj.GetComponent<MeshFilter>().sharedMesh;
                Mesh m_sharedMesh_2 = GameObject.Instantiate(m_sharedMesh);

                m_sharedMesh_2.vertices = frameDatasWS[AnimIdx][f].animVertex;
                m_sharedMesh_2.normals = frameDatasWS[AnimIdx][f].animVNormal;

                q_obj.GetComponent<MeshFilter>().sharedMesh = m_sharedMesh_2;

                //// update visibility
                ls_v_arr = frameDatasWS_flipZ[AnimIdx][f].animVertex;
                ls_vn_arr = frameDatasWS_flipZ[AnimIdx][f].animVNormal;
                tn_arr = frameTNormalWS_flipZ[AnimIdx][f];

                if (SceneManager.checkFrame % 8 == 0)
                    GetVisibility(0);
                else if (SceneManager.checkFrame % 2 == 0)
                    GetVisibility(weld_v_all_arr.Length - 121);

                isVisibilityNeedToBeBuilt = true;
                //SceneManager.checkFrame++;
            }

            if (SceneManager.onClickAddFrame == true)
            {
                if (currentf < objAnimOSList[AnimIdx].frameDatas.Length - 1)
                    currentf++;
                else
                    currentf = 0;

                Debug.Log("frame:" + currentf);
                Mesh m_sharedMesh = q_obj.GetComponent<MeshFilter>().sharedMesh;
                Mesh m_sharedMesh_2 = GameObject.Instantiate(m_sharedMesh);

                m_sharedMesh_2.vertices = frameDatasWS[AnimIdx][currentf].animVertex;
                m_sharedMesh_2.normals = frameDatasWS[AnimIdx][currentf].animVNormal;

                q_obj.GetComponent<MeshFilter>().sharedMesh = m_sharedMesh_2;

                //// update visibility
                ls_v_arr = frameDatasWS_flipZ[AnimIdx][currentf].animVertex;
                ls_vn_arr = frameDatasWS_flipZ[AnimIdx][currentf].animVNormal;
                tn_arr = frameTNormalWS_flipZ[AnimIdx][currentf];

                GetVisibility(0);

                SceneManager.onClickAddFrame = false;
                isVisibilityNeedToBeBuilt = true;
            }
        }

        void GetWeldVertex(ref List<int> weld_vtIdx, ref List<int> weld_v_all, ref List<int> weld_v_allInOne, ref List<int> weld_v_perSize, ref List<int> weld_vtIdx_map, ref List<int> weld_vSize)
        {
            int[] weld_vtIdxArray = new int[vtIdx.Count];
            weld_vtIdx = new List<int>(weld_vtIdxArray);
            int[] weld_vArray = new int[vtIdx.Count];
            weld_v_all = new List<int>(weld_vArray);
            int[] weld_v_allInOneArray = new int[vtIdx.Count];
            weld_v_allInOne = new List<int>(weld_v_allInOneArray);
            int[] weld_vPerSizeArray = new int[vtIdx.Count];
            weld_v_perSize = new List<int>(weld_vPerSizeArray);
            int[] weld_vtIdx_mapArray = new int[ls_v.Count];
            weld_vtIdx_map = new List<int>(weld_vtIdx_mapArray);
            int[] weld_vSizeArray = new int[3];
            weld_vSize = new List<int>(weld_vSizeArray);

            unsafe
            {
                int nv = ls_v.Count;
                int nf = tn.Count;

                fixed (Vector3* ls_vPosPtr = ls_v.ToArray())                // input
                fixed (int* vtIdxPtr = vtIdx.ToArray())                     // input
                fixed (int* weld_vtIdxPtr = weld_vtIdx.ToArray())               // inout
                fixed (int* weld_vPtr = weld_v_all.ToArray())                   // inout
                fixed (int* weld_v_allInOnePtr = weld_v_allInOne.ToArray())     // inout
                fixed (int* weld_vPerSizePtr = weld_v_perSize.ToArray())        // inout
                fixed (int* weld_vtIdx_mapPtr = weld_vtIdx_map.ToArray())       // inout
                fixed (int* weld_vSizePtr = weld_vSize.ToArray())               // inout
                {
                    //weld_func(ls_vPosPtr, vtIdxPtr, nv, nf, weld_vtIdxPtr, weld_vPtr, weld_v_allInOnePtr, weld_vPerSizePtr, weld_vSizePtr);
                    weld_func_ab(ls_vPosPtr, vtIdxPtr, nv, nf, weld_vtIdxPtr, weld_vPtr, weld_v_allInOnePtr, weld_vPerSizePtr, weld_vtIdx_mapPtr, weld_vSizePtr);
                    weld_vtIdxArray = q_IO.ConvertPointerToArray(weld_vtIdxPtr, vtIdx.Count);
                    weld_vArray = q_IO.ConvertPointerToArray(weld_vPtr, vtIdx.Count);
                    weld_v_allInOneArray = q_IO.ConvertPointerToArray(weld_v_allInOnePtr, vtIdx.Count);
                    weld_vPerSizeArray = q_IO.ConvertPointerToArray(weld_vPerSizePtr, vtIdx.Count);
                    weld_vtIdx_mapArray = q_IO.ConvertPointerToArray(weld_vtIdx_mapPtr, ls_v.Count);
                    weld_vSizeArray = q_IO.ConvertPointerToArray(weld_vSizePtr, 3);
                }
            }

            // Resize the array of after-weld
            int[] resized_weld_vArr = new int[weld_vSizeArray[0]];
            int[] resized_weld_v_allInOneArr = new int[weld_vSizeArray[1]];
            int[] resized_weld_vPerSizeArr = new int[weld_vSizeArray[2]];

            Array.Copy(weld_vArray, resized_weld_vArr, weld_vSizeArray[0]);
            Array.Copy(weld_v_allInOneArray, resized_weld_v_allInOneArr, weld_vSizeArray[1]);
            Array.Copy(weld_vPerSizeArray, resized_weld_vPerSizeArr, weld_vSizeArray[2]);

            weld_vtIdx = new List<int>(weld_vtIdxArray);
            weld_v_all = new List<int>(resized_weld_vArr);
            weld_v_allInOne = new List<int>(resized_weld_v_allInOneArr);
            weld_v_perSize = new List<int>(resized_weld_vPerSizeArr);
            weld_vtIdx_map = new List<int>(weld_vtIdx_mapArray);
            weld_vSize = new List<int>(weld_vSizeArray);
        }

        void GetEdgeInfo(ref List<Vector4> vfEdges, ref List<int> fEdges, ref List<Vector4> vfEdgesMap, ref List<int> edgeSize)
        {
            Vector4[] vfEdgesArray = new Vector4[1024 * 1024];
            vfEdges = new List<Vector4>(vfEdgesArray);
            int[] fEdgesArray = new int[vtIdx.Count];
            fEdges = new List<int>(fEdgesArray);
            Vector4[] vfEdgesMapArray = new Vector4[1024 * 1024];
            vfEdgesMap = new List<Vector4>(vfEdgesMapArray);
            int[] edgeSizeArray = new int[1];
            edgeSize = new List<int>(edgeSizeArray);

            unsafe
            {
                int nv = weld_v_all.Count;
                int nf = tn.Count;

                fixed (int* weld_vtIdxPtr = weld_vtIdx.ToArray())               // input
                fixed (int* weld_v_allPtr = weld_v_all.ToArray())               // input
                fixed (Vector4* vfEdgesPtr = vfEdges.ToArray())                 // output
                fixed (int* fEdgesPtr = fEdges.ToArray())                       // output
                fixed (Vector4* vfEdgesMapPtr = vfEdgesMap.ToArray())           // output
                fixed (int* edgeSizePtr = edgeSize.ToArray())                   // output
                {
                    //cal_edge_func(weld_vtIdxPtr, nv, nf, vfEdgesPtr, fEdgesPtr, edgeSizePtr);
                    cal_edge_func_ab(weld_vtIdxPtr, weld_v_allPtr, nv, nf, nv, vfEdgesPtr, fEdgesPtr, vfEdgesMapPtr, edgeSizePtr);
                    vfEdgesArray = q_IO.ConvertPointerToArray(vfEdgesPtr, 1024 * 1024);
                    fEdgesArray = q_IO.ConvertPointerToArray(fEdgesPtr, vtIdx.Count);
                    vfEdgesMapArray = q_IO.ConvertPointerToArray(vfEdgesMapPtr, 1024 * 1024);
                    edgeSizeArray = q_IO.ConvertPointerToArray(edgeSizePtr, 1);
                }
            }

            Vector4[] resized_vfEdgesArr = new Vector4[edgeSizeArray[0]];
            Vector4[] resized_vfEdgesMapArr = new Vector4[edgeSizeArray[0]];
            Array.Copy(vfEdgesArray, resized_vfEdgesArr, edgeSizeArray[0]);
            Array.Copy(vfEdgesMapArray, resized_vfEdgesMapArr, edgeSizeArray[0]);
            vfEdges = new List<Vector4>(resized_vfEdgesArr);
            vfEdgesMap = new List<Vector4>(resized_vfEdgesMapArr);
            fEdges = new List<int>(fEdgesArray);
            edgeSize = new List<int>(edgeSizeArray);

        }

        void GetVisibility(int first_vi)
        {
            int nf = tn.Count;
            int dnv = ls_v.Count;
            int nv = weld_v_all.Count;
            int ne = vfEdges.Count;

            if (des_ei_arr.Length < 1024 * 64)
            {
                Array.Resize(ref des_ei_arr, 1024 * 64);
                Array.Resize(ref des_es_arr, 1024 * 64);
            }

            unsafe
            {
                fixed (Vector3* ls_vPtr = ls_v_arr)
                fixed (Vector3* ls_vnPtr = ls_vn_arr)
                fixed (int* vtIdxPtr = vtIdx_arr)
                fixed (int* weld_vtIdxPtr = weld_vtIdx_arr)
                fixed (int* weld_v_allPtr = weld_v_all_arr)
                fixed (Vector3* tnPtr = tn_arr)                                    // face normal of non-weld model
                fixed (Vector4* vfEdgesPtr = vfEdges_arr)                          // face normal of non-weld model
                fixed (Vector4* vfEdgesMapPtr = vfEdgesMap_arr)
                fixed (int* fEdgesPtr = fEdges_arr)

                fixed (int* des_eiPtr = des_ei_arr)
                fixed (float* des_esPtr = des_es_arr)
                fixed (int* des_vinfoPtr = des_vinfo_arr)
                fixed (int* vis_sizePtr = vis_size_arr)
                {
                    gen_tempVecVis_ondemand_ac(ls_vPtr, ls_vnPtr, vtIdxPtr, weld_vtIdxPtr,
                                           weld_v_allPtr, tnPtr, vfEdgesPtr, vfEdgesPtr, fEdgesPtr,
                                           nf, dnv, nv, ne, first_vi,
                                           des_eiPtr, des_esPtr, des_vinfoPtr, vis_sizePtr);

                    //des_eiArray = q_IO.ConvertPointerToArray(des_eiPtr, 1024 * 64 * nv);     // need to be resized using vis_sizeArray[0]
                    //des_esArray = q_IO.ConvertPointerToArray(des_esPtr, 1024 * 64 * nv);     // need to be resized using vis_sizeArray[0]
                    //des_vinfoArray = q_IO.ConvertPointerToArray(des_vinfoPtr, nv + 1);
                    //vis_sizeArray = q_IO.ConvertPointerToArray(vis_sizePtr, 2);
                }
            }

            Array.Resize(ref des_ei_arr, vis_size_arr[0]);
            Array.Resize(ref des_es_arr, vis_size_arr[0]);

            //int[] resized_des_eiArray = new int[vis_sizeArray[0]];
            //float[] resized_des_esArray = new float[vis_sizeArray[0]];
            //Array.Copy(des_eiArray, resized_des_eiArray, vis_sizeArray[0]);
            //Array.Copy(des_esArray, resized_des_esArray, vis_sizeArray[0]);
            //des_ei = new List<int>(resized_des_eiArray);
            //des_es_float = new List<float>(resized_des_esArray);
            //des_vinfo = new List<int>(des_vinfoArray);
            //vis_size = new List<int>(vis_sizeArray);
        }

        void Clear() 
        {
            ls_v.Clear();
            ls_vn.Clear();
            vtIdx.Clear();
            tn.Clear();

            weld_vtIdx.Clear();
            weld_v_all.Clear();
            weld_v_allInOne.Clear();
            weld_v_perSize.Clear();
            weld_vSize.Clear();

            vfEdges.Clear();
            fEdges.Clear();
            vfEdgesMap.Clear();
            edgeSize.Clear();

            des_ei.Clear();
            des_es_float.Clear();
            des_vibility.Clear();
            des_vis_size.Clear();

            des_vis_single.Clear();
            vis_single_size.Clear();

            frameDatasWS?.Clear();
            frameTNormalWS?.Clear();
            objAnimOSList?.Clear();

            ls_vBuffer?.Release();
            ls_vnBuffer?.Release();
            vtIdxBuffer?.Release();
            weld_vtIdxBuffer?.Release();
            weld_vtIdx_mapBuffer?.Release();
            vfEdgeMapBuffer?.Release();
            deseiBuffer?.Release();
            desesBuffer?.Release();
            desVibilityBuffer?.Release();

            foreach (var pair in deseiBufferPairs)
            {
                pair.Value.Release();
            }
            deseiBufferPairs.Clear();

            foreach (var pair in desesBufferPairs)
            {
                pair.Value.Release();
            }
            desesBufferPairs.Clear();

            foreach (var pair in desVibilityBufferPairs)
            {
                pair.Value.Release();
            }
            desVibilityBufferPairs.Clear();

            foreach (var pair in vfEdgeMapBufferPairs)
            {
                pair.Value.Release();
            }
            vfEdgeMapBufferPairs.Clear();

            foreach (var pair in weld_vtIdxBufferPairs)
            {
                pair.Value.Release();
            }
            weld_vtIdxBufferPairs.Clear();

            foreach (var pair in ls_vBufferPairs)
            {
                pair.Value.Release();
            }
            ls_vBufferPairs.Clear();

            foreach (var pair in ls_vnBufferPairs)
            {
                pair.Value.Release();
            }
            ls_vnBufferPairs.Clear();

            foreach (var pair in weld_v_allPairs)
            {
                pair.Value.Release();
            }
            weld_v_allPairs.Clear();

            foreach (var pair in vtIdxPairs)
            {
                pair.Value.Release();
            }
            vtIdxPairs.Clear();

            foreach (var pair in weld_vtIdx_mapBufferPairs)
            {
                pair.Value.Release();
            }
            weld_vtIdx_mapBufferPairs.Clear();
            
        }
    }
}