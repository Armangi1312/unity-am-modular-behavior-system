using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform Target;
    [SerializeField] private float Speed;

    private void LateUpdate()
    {
        if (Target == null) return;

        transform.position = Vector3.Lerp(transform.position, Target.position, Speed * Time.deltaTime);
    }
}
