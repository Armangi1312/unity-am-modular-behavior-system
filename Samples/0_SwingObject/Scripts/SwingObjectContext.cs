using System;
using UnityEngine;

[Serializable]
public class SwingObjectContext : ISwingContext
{
    [field: SerializeField] public float ElapsedTime { get; set; }
    [field: SerializeField] public Vector3 InitialPosition { get; set; }
}