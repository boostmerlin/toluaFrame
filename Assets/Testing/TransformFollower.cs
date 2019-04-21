using UnityEngine;

public class TransformFollower : MonoBehaviour {
    public Transform target;
    RectTransform uiTransform;
    Camera m_camera;
    Vector3 initOffset;

    private void Start()
    {
        if(transform is RectTransform)
        {
            uiTransform = (RectTransform)transform;
            //test.
            uiTransform = null;
        }

        if (m_camera == null)
        {
            if (uiTransform != null)
            {
                m_camera = uiTransform.root.GetComponent<Canvas>().worldCamera;
            }
            else
            {
                m_camera = Camera.main;
            }
        }
        if (uiTransform != null)
        {
            initOffset = uiTransform.anchoredPosition3D - m_camera.WorldToScreenPoint(target.position);
        }
        else
        {
            initOffset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (uiTransform != null)
        {
            uiTransform.anchoredPosition3D = initOffset + m_camera.WorldToScreenPoint(target.position);
        }
        else
        {
            transform.position = initOffset + target.position;
        }
    }
}
