using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExampleUnityGame
{
    public class AlignToGrid : MonoBehaviour
    {
        void Start()
        {
            transform.position = GridSystem.Instance.GetNearestValidStepPosition(transform.position);
        }
    }
}