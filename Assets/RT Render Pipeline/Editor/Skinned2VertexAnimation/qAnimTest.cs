using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class qAnimTest : MonoBehaviour
{
    public qVtAnimData _vtAnimData;
    [Range(0, 60)]
    public int current_frame = 0;

    private Mesh _mesh;
    private Matrix4x4[] _bindPoses;
    private List<Vector3> _srcPoints;
    private List<Vector3> _newPoints;


    void Awake()
    {
        _mesh = GetComponentInChildren<MeshFilter>().mesh;
        _srcPoints = new List<Vector3>();
        _mesh.GetVertices(_srcPoints);
        _bindPoses = _mesh.bindposes;
        _newPoints = new List<Vector3>(_srcPoints);
        //Load();
    }

    void Load()
    {
        if (_vtAnimData == null)
        {
            Debug.Log("Can not find the animation data!");
            return;
        }

        Debug.Log(_vtAnimData.frameDatas.Length);
        Play(0);
    }

    void Play(int frame)
    {
        _mesh.SetVertices(_vtAnimData.frameDatas[frame].animVertex);
        _mesh.SetNormals(_vtAnimData.frameDatas[frame].animVNormal);
    }

    void Update()
    {
        //_time += Time.deltaTime;
        //_time %= _vtAnimData.animLen;
        //int f = (int)(_time / (1.0f / _vtAnimData.frame));
        //if (f != _frame)
        //{
        //    Play(f);
        //}

        //Play(1);
    }
}
