using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AM.Core.Utilities
{
    [Serializable]
    public sealed class SceneObjectReference<T> where T : UnityEngine.Object
    {
        [SerializeField, HideInInspector] private string targetID;
        [SerializeField, HideInInspector] private T target;

        public T Value
        {
            get => target;
            set
            {
                target = value;
#if UNITY_EDITOR
                SyncID();
#endif
            }
        }

#if UNITY_EDITOR
        public void Resolve()
        {
            if (target != null)
                return;

            if (string.IsNullOrEmpty(targetID))
                return;

            if (GlobalObjectId.TryParse(targetID, out var id))
                target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as T;
            else
                target = null;
        }

        private void SyncID()
        {
            if (target == null)
            {
                targetID = null;
                return;
            }

            var id = GlobalObjectId.GetGlobalObjectIdSlow(target);
            targetID = id.identifierType != 0 ? id.ToString() : null;
        }
#endif
    }
}
