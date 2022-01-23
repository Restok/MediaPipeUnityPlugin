using System;
using UnityEngine;

namespace LandmarkInterface
{
    public class LandmarkOrientation : MonoBehaviour
    {


    private void updateOrientation()
        {
            float x = 0, y = 0, z = 0;
            z = (0 - 90);



      this.transform.localEulerAngles = new Vector3(x, y, z);
        }

    }
}
