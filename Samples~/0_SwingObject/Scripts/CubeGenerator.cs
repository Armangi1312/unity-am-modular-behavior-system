using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
    public GameObject Cube;

    public int XSize;
    public int ZSize;

    private void Awake()
    {
        for (int i = 0; i < XSize; i++)
        {
            for (int j = 0; j < ZSize; j++)
            {
                Instantiate(Cube, new Vector3(i, 0f, j), Quaternion.identity);
            }
        }
    }
}
