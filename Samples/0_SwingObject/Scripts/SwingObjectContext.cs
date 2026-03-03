using AM.Core;
using System;
using UnityEngine;

[Serializable]
public class SwingObjectContext : IContext
{
    [field: SerializeField] public float ElapsedTime { get; set; }
}