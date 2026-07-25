using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Camera))]
    public class CameraSizeByAspect : MonoBehaviour
    {
        [SerializeField] private float baseOrthographicSize = 5f;
        [SerializeField] private float referenceAspect = 16f / 9f;

        private Camera cam;
        private int lastWidth;
        private int lastHeight;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            ApplySize();
        }

        private void Update()
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight)
                ApplySize();
        }

        private void ApplySize()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            float aspect = (float)lastWidth / lastHeight;
            cam.orthographicSize = baseOrthographicSize * (referenceAspect / aspect);
        }
    }
}