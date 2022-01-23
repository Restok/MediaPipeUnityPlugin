using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.CoordinateSystem;

namespace LandmarkInterface
{
  public class HandLandmark : MonoBehaviour
  {
    public LandmarkResultSet landmarkSet;
    public List<GameObject> LandmarkObjects;
    private Transform[] allChildren;
    public LandmarkType LandmarkType;
    public bool FlipXY = false;
    public bool NegateX = false;
    public bool NegateY = false;

    [SerializeField]
    private float thumbModelLength = 0.03f;
    private float scale;

    private DepthCalibrator depthCalibrator = new DepthCalibrator(-0.0719f, 0.439f);
    private TransformLink[] transformLinkers;
    private void Awake()
    {
      transformLinkers = this.transform.GetComponentsInChildren<TransformLink>();
    }

    private void Update()
    {
      ; var list = landmarkSet.GetLandmarks(this.LandmarkType);
      if (list != null && list.Count != 0)
      {
        updateLandmarkPosition(list);
      }

      foreach (var linker in transformLinkers)
      {
        linker.UpdateTransform();
      }

    }

    private void OnDrawGizmos()
    {
      //for debugging Positions
      Gizmos.color = Color.red;
      for (int i = 0; i < LandmarkObjects.Count; i++)
      {
        Gizmos.DrawSphere(LandmarkObjects[i].transform.position, 0.005f);
      }
    }


    private void updateLandmarkPosition(List<Vector3> landmarks)
    {

      Vector3 newScale = new Vector3(1, 1, 1);
      var offset = landmarks[0];
      if (NegateY)
      {
        offset.y *= -1;
        offset.y += 1;
      }
      var poseLandmarks = landmarkSet.GetLandmarks(LandmarkType.Pose);
      if (poseLandmarks != null)
      {
        if (this.LandmarkType == LandmarkType.LeftHand)
        {
          var elbowpos = poseLandmarks[13];
          landmarks.Add(elbowpos);
        }
        else if (this.LandmarkType == LandmarkType.RightHand)
        {
          var elbowpos = poseLandmarks[14];
          landmarks.Add(elbowpos);
        }
      }
      for (int i = 1; i < landmarks.Count; i++)
      {
        var x = landmarks[i].x-offset.x;
        var y = landmarks[i].y-offset.y;
        var z = landmarks[i].z-offset.z;
        if (i == landmarks.Count - 1 || LandmarkType == LandmarkType.Pose)
        {
          x = landmarks[i].x;
          y = landmarks[i].y;
          z = landmarks[i].z;
        }
        if (x == 0 && y == 0 && z == 0)
          return;

        if (NegateX) x *= -1;
        if (NegateY) y *= -1;
        if (FlipXY) (x, y) = (y, x);
        try { LandmarkObjects[i].transform.localPosition = new Vector3((x*1.78f), y, z); }
        catch
        {
          Debug.Log(this.LandmarkType);
          Debug.Log("count of gmobj:" + LandmarkObjects.Count);
          Debug.Log("index: " + i);
        }

      }

      float depth = depthCalibrator.GetDepthFromThumbLength(scale);

      this.transform.localPosition = new Vector3((offset.x*1.8f),offset.y,depth*scale);
      updateCharHand(LandmarkObjects, this.LandmarkType);

    }
    private void updateCharHand(List<GameObject> landmarkObjects, LandmarkType type)
    {
      if(type == LandmarkType.LeftHand) { allChildren = GameObject.Find("B-hand_L").GetComponentsInChildren<Transform>(); }
      else
      {
        allChildren = GameObject.Find("B-hand_R").GetComponentsInChildren<Transform>();
      }
      var wristTransform = LandmarkObjects[0].transform;
      var indexFinger = LandmarkObjects[5].transform.position;
      var middleFinger = LandmarkObjects[9].transform.position;

      var vectorToMiddle = middleFinger - wristTransform.position;
      var vectorToIndex = indexFinger - wristTransform.position;
      //to get ortho vector of middle finger from index finger
      Vector3.OrthoNormalize(ref vectorToMiddle, ref vectorToIndex);

      //vector normal to wrist
      Vector3 normalVector = Vector3.Cross(vectorToIndex, vectorToMiddle);
      Debug.DrawRay(allChildren[0].position, normalVector, Color.white);
      Debug.DrawRay(allChildren[0].position, vectorToIndex, Color.yellow);
      allChildren[0].transform.rotation = Quaternion.LookRotation(vectorToIndex * -1, normalVector);

      int[] ind = new int[] { 2, 5, 9, 13, 17 };
      for(int i =0; i < 21; i++)
      {
        if (i % 4 == 0) { continue; }
        if ((i - 1) % 4 == 0) {
          Vector3 vec1 = landmarkObjects[0].transform.position - landmarkObjects[i].transform.position;
          Vector3 vec2 = landmarkObjects[i].transform.position - landmarkObjects[i+1].transform.position;
          Debug.Log(ind[(i-1) / 4]);
          allChildren[ind[(i-1) / 4]].localRotation = Quaternion.FromToRotation(vec1, vec2);
        }
        else
        {
          //Vector3 vec1 = landmarkObjects[i - 1].transform.position - landmarkObjects[i].transform.position;
          //Vector3 vec2 = landmarkObjects[i].transform.position - landmarkObjects[i + 1].transform.position;
          
          //allChildren[ind[i / 4]+].position = new Vector3(0, 0, Vector3.Angle(vec1, vec2);

        }

        updateWristRotation();


      }



    }

    //correct landmark scale based on thumb length
    private void updateLandmarkScale(List<Vector3> list)
    {
      var pointA = new Vector3(list[0].x, list[0].y, list[0].z);
      var pointB = new Vector3(list[1].x, list[1].y, list[1].z);
      var thumbDetectedLength = Vector3.Distance(pointA, pointB);
      if (thumbDetectedLength == 0)
        return;
      scale = thumbModelLength / thumbDetectedLength;
      this.transform.localScale = new Vector3(scale, scale, scale);
    }

    private void updateWristRotation()
    {

    }

  }

}
