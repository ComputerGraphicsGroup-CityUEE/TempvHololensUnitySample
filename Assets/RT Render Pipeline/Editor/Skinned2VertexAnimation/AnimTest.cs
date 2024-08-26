using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimTest : MonoBehaviour
{
    public string animName = "walk";
    public string animDir = "Assets/Resources/Animation/GLTF";
    [Range(0, 60)]
    public int current_frame = 0;

    private AnimData _animData;
    private float _time;
    private int _frame;
    private Mesh _mesh;
    private Matrix4x4[] _bindPoses;
    private List<Vector3> _srcPoints;
    private List<Vector3> _newPoints;

    private List<List<Vector3>> vertexAnims;
    // private int last_frame = 0;


    void Awake()
    {
        _mesh = GetComponentInChildren<MeshFilter>().mesh;
        _srcPoints = new List<Vector3>();
        _mesh.GetVertices(_srcPoints);
        _bindPoses = _mesh.bindposes;
        _newPoints = new List<Vector3>(_srcPoints);
        vertexAnims = new List<List<Vector3>>();
        Load(animName);
    }

    void Load(string name)
    {
        if (_animData == null || _animData.name != name)
        {
            string path = animDir + "/" + name;
            string resName = path.Substring("Assets/Resources/".Length);
            _animData = Resources.Load<AnimData>(resName);
            Debug.Log(resName);
            _time = 0.0f;
            _frame = -1;
        }
        if (_animData == null)
        {
            Debug.Log("Can not find the animation data in Assets/Resources/" + _animData);
            return;
        }

        Debug.Log(_animData.frameDatas.Length);
        for (int i = 0; i < _animData.frameDatas.Length; i++)
        {
            List<Vector3> tempPoints = GetVertexPosFromAnims(i);
            vertexAnims.Add(tempPoints);
        }
        Play(0);
    }

    void Play(int frame)
    {
        _mesh.SetVertices(vertexAnims[frame]);
    }

    void Update()
    {
        _time += Time.deltaTime;
        _time %= _animData.animLen;
        int f = (int)(_time / (1.0f / _animData.frame));
        if (f != _frame)
        {
            Play(f);
        }

        //if (current_frame != last_frame)
        //{
        //    Debug.Log(current_frame);
        //    Play(current_frame);
        //    last_frame = current_frame;
        //}
    }

    List<Vector3> GetVertexPosFromAnims(int _frame)
    {
        List<Vector3> _tempPoints = new List<Vector3>();

        AnimData.FrameData frameData = _animData.frameDatas[_frame];
        for (int i = 0; i < _srcPoints.Count; ++i)
        {
            Vector3 point = _srcPoints[i];
            BoneWeight weight = _mesh.boneWeights[i];
            Matrix4x4 tempMat0 = frameData.matrix4X4s[weight.boneIndex0] * _bindPoses[weight.boneIndex0];
            Matrix4x4 tempMat1 = frameData.matrix4X4s[weight.boneIndex1] * _bindPoses[weight.boneIndex1];
            Matrix4x4 tempMat2 = frameData.matrix4X4s[weight.boneIndex2] * _bindPoses[weight.boneIndex2];
            Matrix4x4 tempMat3 = frameData.matrix4X4s[weight.boneIndex3] * _bindPoses[weight.boneIndex3];

            Vector3 temp = tempMat0.MultiplyPoint(point) * weight.weight0 +
                                   tempMat1.MultiplyPoint(point) * weight.weight1 +
                                   tempMat2.MultiplyPoint(point) * weight.weight2 +
                                   tempMat3.MultiplyPoint(point) * weight.weight3;

            _tempPoints.Add(temp);
        }

        return _tempPoints;
    }


    #region other's code
    void Play(string name)
    {
        if (_animData == null || _animData.name != name)
        {
            string path = animDir + "/" + name;
            string resName = path.Substring("Assets/Resources/".Length);
            _animData = Resources.Load<AnimData>(resName);
            _time = 0.0f;
            _frame = -1;
        }
    }

    // Update is called once per frame
    void Update_old()
    {
        if (_animData == null)
        {
            Debug.Log("Can not find the animation data in Assets/Resources/" + _animData);
            return;
        }
        if (_frame < 0)
        {
            ApplyFrame(0);
            return;
        }
        _time += Time.deltaTime;
        _time %= _animData.animLen;
        int f = (int)(_time / (1.0f / _animData.frame));

        if (f != _frame)
        {
            ApplyFrame(f);
        }
    }

    void ApplyFrame(int f)
    {
        _frame = f;

        AnimData.FrameData frameData = _animData.frameDatas[_frame];
        for (int i = 0; i < _srcPoints.Count; ++i)
        {
            Vector3 point = _srcPoints[i];
            BoneWeight weight = _mesh.boneWeights[i];
            Matrix4x4 tempMat0 = frameData.matrix4X4s[weight.boneIndex0] * _bindPoses[weight.boneIndex0];
            Matrix4x4 tempMat1 = frameData.matrix4X4s[weight.boneIndex1] * _bindPoses[weight.boneIndex1];
            Matrix4x4 tempMat2 = frameData.matrix4X4s[weight.boneIndex2] * _bindPoses[weight.boneIndex2];
            Matrix4x4 tempMat3 = frameData.matrix4X4s[weight.boneIndex3] * _bindPoses[weight.boneIndex3];

            Vector3 temp = tempMat0.MultiplyPoint(point) * weight.weight0 +
                                   tempMat1.MultiplyPoint(point) * weight.weight1 +
                                   tempMat2.MultiplyPoint(point) * weight.weight2 +
                                   tempMat3.MultiplyPoint(point) * weight.weight3;

            _newPoints[i] = temp;
        }

        _mesh.SetVertices(_newPoints);
    }
    #endregion
}
