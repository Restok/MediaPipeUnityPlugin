using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.CoordinateSystem;
namespace Mediapipe.Unity.Holistic
{
  public class hand_rig : MonoBehaviour
  {
    private static readonly string[] _fbxHandSidePrefix = { "_L", "_R" };
    private static readonly string _fbxHandBonePrefix = "B-f_";
    public string boneName;
    public Vector3 vec1;
    public Vector3 vec2;
    private static readonly string[] _fbxHandBoneNames =
    {
    "hand",
    "thumb_01",
    "thumb_02",
    "thumb_03",
    "index_01",
    "index_02",
    "index_03",
    "middle_01",
    "middle_02",
    "middle_03",
    "ring_01",
    "ring_02",
    "ring_03",
    "pinky_01",
    "pinky_02",
    "pinky_03"
  };

    public void UpdateHand(NormalizedLandmarkList landmarksList, int handIndex, NormalizedLandmarkList pose)
    {
      int counter = 1;
      Vector3 wristPos = RealWorldCoordinate.GetLocalPosition(landmarksList.Landmark[0].X, landmarksList.Landmark[0].Y, landmarksList.Landmark[0].Z, new Vector3(1, 1, 1), isMirrored: false);
      Vector3 elbowPos = RealWorldCoordinate.GetLocalPosition(pose.Landmark[14].X, pose.Landmark[14].Y, pose.Landmark[14].Z, new Vector3(1, 1, 1), isMirrored: false);
      Vector3 knuckle1 = RealWorldCoordinate.GetLocalPosition(landmarksList.Landmark[5].X, landmarksList.Landmark[5].Y, landmarksList.Landmark[5].Z, new Vector3(1, 1, 1), isMirrored: false);
      Vector3 knuckle2 = RealWorldCoordinate.GetLocalPosition(landmarksList.Landmark[13].X, landmarksList.Landmark[13].Y, landmarksList.Landmark[13].Z, new Vector3(1, 1, 1), isMirrored: false);
      Vector3 elbowWrist = elbowPos - wristPos;
      Vector3 wristKnuckle2 = wristPos - knuckle2;
      Quaternion transformQuat = Quaternion.FromToRotation(elbowWrist, Vector3.forward);
      wristKnuckle2 = transformQuat * wristKnuckle2;
      boneName = "B-" + "forearm"+ _fbxHandSidePrefix[handIndex];

      Vector3 normalPlane = transformQuat* Vector3.Cross((wristPos - knuckle1),  (wristPos - knuckle2)).normalized;
      //GameObject.Find(boneName).transform.localEulerAngles = new Vector3(Vector3.SignedAngle(Vector3.ProjectOnPlane(normalPlane, Vector3.forward), Vector3.up, Vector3.forward), 0, 0);

      boneName = "B-" + _fbxHandBoneNames[0] + _fbxHandSidePrefix[handIndex];

      GameObject.Find(boneName).transform.localEulerAngles = new Vector3(0, Vector3.SignedAngle(Vector3.ProjectOnPlane(wristKnuckle2, Vector3.forward),Vector3.up, Vector3.forward), Vector3.SignedAngle(Vector3.forward, wristKnuckle2, Vector3.left));

      for (int i = 1; i < 21; i++)
      {
        if (i % 4 == 0) { continue; }
        Vector3 pos1 = RealWorldCoordinate.GetLocalPosition(landmarksList.Landmark[i].X, landmarksList.Landmark[i].Y, landmarksList.Landmark[i].Z, new Vector3(1, 1, 1), isMirrored: false);
        Vector3 pos2 = RealWorldCoordinate.GetLocalPosition(landmarksList.Landmark[i + 1].X, landmarksList.Landmark[i + 1].Y, landmarksList.Landmark[i + 1].Z, new Vector3(1, 1, 1), isMirrored: false);
        Vector3 pos3 = RealWorldCoordinate.GetLocalPosition(landmarksList.Landmark[i - 1].X, landmarksList.Landmark[i - 1].Y, landmarksList.Landmark[i - 1].Z, new Vector3(1, 1, 1), isMirrored: false);
        boneName = _fbxHandBonePrefix + _fbxHandBoneNames[counter] + _fbxHandSidePrefix[handIndex];
        if ((i - 1) % 4 == 0)
        {
          if (i - 1 == 0)
          {
            vec1 = wristPos - knuckle2;
            vec2 = wristPos-pos1;
            Quaternion transformFinger = Quaternion.FromToRotation(vec1, Vector3.forward);
            vec2 = transformFinger * vec2;
            GameObject.Find(boneName).transform.localRotation = Quaternion.FromToRotation(Vector3.forward, vec2);
          }
          else
          {
            vec1 = wristPos - pos1;
            vec2 = pos1 - pos2;
            //Quaternion transformFinger = Quaternion.FromToRotation(vec1, Vector3.left);
            //vec2 = transformFinger * vec2;
            GameObject.Find(boneName).transform.localEulerAngles = new Vector3(0, 0, Vector3.SignedAngle(vec1, vec2, Vector3.Cross(vec1, vec2)));
          }

        }
        else
        {
          vec1 = pos3 - pos1;
          vec2 = pos1 - pos2;
          GameObject.Find(boneName).transform.localEulerAngles = new Vector3(0, 0, Vector3.SignedAngle(vec1, vec2, Vector3.Cross(vec1, vec2)));

        }
        counter += 1;
      }
    }
  }
}
