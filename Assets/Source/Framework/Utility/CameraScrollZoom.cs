using UnityEngine;
using Lean.Touch;

public class CameraScrollZoom : MonoBehaviour
{
    public interface IScrollZoomEvent
    {
        void OnMoveEvent(int state, bool isSnap);
        void OnMapZoom(int state, bool isSnap);
        void OnViewRectChange();
    }

    public LeanScreenDepth ScreenDepth;

    public bool freezeMove = false;
    public bool freezeZoom = false;

    public const int BEGIN = 1;
    public const int END = 2;

    public bool isSnapMove { get; private set; }
    public float smoothTime = 0.25f;
    public float snapMoveMutiply = 9.23f;
    public float snapZoomMutiply = 2f;
    public float touchSensitivity = 2.5f;
    public bool isSmoothZoom = false;
    public bool isSmoothMove = false;
    public bool tryingMove = false;

    IScrollZoomEvent scrollZoomEvent;

    public bool IgnoreTouch
    {
        get
        {
            return ignoreTouch;
        }
        set
        {
            ignoreTouch = value;
            //moveState = 0;
        }
    }

    public Camera Camera
    {
        get
        {
            return cam;
        }
        set
        {
            cam = value;
            startScreentPos = cam.WorldToScreenPoint(transform.position);
        }
    }

    public float MaxSize
    {
        get
        {
            return _maxSize;
        }
        set
        {
            if (maxRangeRect == Rect.zero)
            {
                //
                return;
            }
            float cw = Mathf.Min(maxRangeRect.width, value * Camera.aspect);
            _maxSize = cw / Camera.aspect;
        }
    }

    public Rect ViewRect;
    public Vector3 actualMoveDelta;

    Camera cam;
    bool IgnoreStartedOverGui = true;
    bool IgnoreIsOverGui = false;
    Vector3 targetPosition;
    Vector3 startScreentPos;
    bool ignoreTouch = false;
    private Vector3 velocity = Vector3.zero;
    protected Rect cameraRect;
    protected Rect maxRangeRect;
    bool isFingerSet;
    bool isFingerDown;
    bool isSwipeMove;
    bool isSnapZoom;
    int moveState = 0;
    int zoomState = 0;

    public float ZoomSensitivity = 0.7f;
    public float MinSize = 1f;
    float _maxSize = 40;

    float currentSize;
    float targetSize;
    bool isMoved = false;

    public void SetScrollZoomEvent(IScrollZoomEvent scrollZoomEvent)
    {
        this.scrollZoomEvent = scrollZoomEvent;
    }

    void moveInvoke(int state, bool isSnap)
    {
        if (scrollZoomEvent != null)
        {
            scrollZoomEvent.OnMoveEvent(state, isSnap);
        }
    }

    void zoomInvoke(int state, bool isSnap)
    {
        if (scrollZoomEvent != null)
        {
            scrollZoomEvent.OnMapZoom(state, isSnap);
        }
    }

    public void SnapToPostion(Vector3 pos, bool transition)
    {
        if (!enabled || freezeMove)
        {
            Debug.LogWarning("Snap no effect for mapmove not enabled.");
            moveInvoke(END, true);
            return;
        }
        pos.z = transform.position.z;
        if (!transition)
        {
            moveInvoke(BEGIN, true);
            transform.position = pos;
            updateCameraRect();
            moveInvoke(END, true);
        }
        else
        {
            targetPosition = pos;
            if (!isMovedToTarget())
            {
                isSnapMove = true;
                moveState = 0;
            }
            else
            {
                moveInvoke(END, true);
            }
        }
    }

    float ZoomSteps = 0;
    float ZoomStart = 0;
    public void SnapToZoom(float v, bool transition)
    {
        targetSize = Mathf.Clamp(v, MinSize, MaxSize);
        isSnapZoom = transition;
        if (targetSize == currentSize || freezeZoom)
        {
            zoomState = 4;
            endZoom();
            return;
        }
        zoomState = 3;
        if (transition)
        {
        }
        else
        {
            currentSize = targetSize;
            beginMove();
            SetZoom();
            endZoom();
        }
        ZoomSteps = 0;
        ZoomStart = currentSize;
    }

    public void SnapToGameObject(GameObject go, bool transition = true)
    {
        Vector3 pos = go.transform.position;
        this.SnapToPostion(pos, transition);
    }

    public Vector2 SetSize(Vector2 position, Vector2 halfSize)
    {
        maxRangeRect = new Rect(position, halfSize);
        return new Vector2(maxRangeRect.width, maxRangeRect.height);
    }

    public Rect GetSize()
    {
        return maxRangeRect;
    }

    private void OnEnable()
    {
        LeanTouch.OnFingerSet += OnFingerSet;
        LeanTouch.OnFingerUp += OnFingerUp;
        LeanTouch.OnFingerDown += OnFingerDown;
        Camera = LeanTouch.GetCamera(null, gameObject);
        currentSize = Camera.orthographicSize;
    }

    private void OnDisable()
    {
        isSnapMove = false;
        isSwipeMove = false;
        isFingerSet = false;
        LeanTouch.OnFingerSet -= OnFingerSet;
        LeanTouch.OnFingerUp -= OnFingerUp;
        LeanTouch.OnFingerDown -= OnFingerDown;
    }

    void OnFingerSet(LeanFinger finger)
    {
        if ((IgnoreIsOverGui && finger.IsOverGui) || finger.StartedOverGui || IgnoreTouch)
        {
            return;
        }
        if (isSnapMove || isSwipeMove || isSnapZoom)
        {
            return;
        }
        if (isFingerDown)
        {
            isFingerDown = false;
            moveState = 0;
        }
        if (!finger.Swipe)
        {
            isFingerSet = true;
        }
        else
        {
            //Debug.LogError("OnFinger Swipe. ");
        }
    }
    void OnFingerDown(LeanFinger finger)
    {
        if ((IgnoreTouch || isSnapMove || isSnapZoom || isSwipeMove) 
            || (IgnoreIsOverGui && finger.IsOverGui) || finger.StartedOverGui)
        {
            return;
        }
        //Debug.LogError("OnFingerDown````");
        isFingerDown = true;
    }
    void OnFingerUp(LeanFinger finger)
    {
        //Debug.Log("OnFingerUp swip? " + finger.Swipe);
        if ((IgnoreIsOverGui && finger.IsOverGui) || finger.StartedOverGui || IgnoreTouch)
        {
            return;
        }

        if (!finger.Tap)
        {
            Vector2 startPos = Camera.WorldToScreenPoint(transform.position);
            var move = ScreenDepth.ConvertDelta(startPos, startPos + (finger.ScreenPosition - finger.StartScreenPosition) / finger.Age * 0.1342f, Camera, gameObject) * touchSensitivity;
            targetPosition = transform.position - move;
            if (!isMovedToTarget())
            {
                isSwipeMove = true;
            }
        }
        else
        {
            endMove();
        }
        isFingerDown = false;
        isFingerSet = false;
    }

    void clampCamera(out bool clampX, out bool clampY)
    {
        //×ó±ß½ç
        float x = transform.position.x;
        float y = transform.position.y;
        clampX = false;
        clampY = false;
        if (cameraRect.x - cameraRect.width < maxRangeRect.x - maxRangeRect.width)
        {
            clampX = true;
            x = maxRangeRect.x - maxRangeRect.width + cameraRect.width;
        }//right corner
        else if (cameraRect.x + cameraRect.width > maxRangeRect.x + maxRangeRect.width)
        {
            clampX = true;
            x = maxRangeRect.x + maxRangeRect.width - cameraRect.width;
        }
        //bottom
        if (cameraRect.y - cameraRect.height < maxRangeRect.y - maxRangeRect.height)
        {
            clampY = true;
            y = maxRangeRect.y - maxRangeRect.height + cameraRect.height;
        }
        else if (cameraRect.y + cameraRect.height > maxRangeRect.x + maxRangeRect.height)
        {
            clampY = true;
            y = maxRangeRect.x + maxRangeRect.height - cameraRect.height;
        }
        transform.position = new Vector3(x, y, transform.position.z);
    }

    void updateCameraRect()
    {
        if (maxRangeRect == Rect.zero)
        {
            return;
        }
        float x = transform.position.x;
        float y = transform.position.y;
        cameraRect.x = x;
        cameraRect.y = y;
        cameraRect.height = Camera.orthographicSize;
        if(cameraRect.height > maxRangeRect.height)
        {
            cameraRect.height = maxRangeRect.height;
        }
        cameraRect.width = cameraRect.height * Camera.aspect;
        if (cameraRect.width > maxRangeRect.width)
        {
            cameraRect.width = maxRangeRect.width;
        }

        bool clampX = false, clampY = false;
        clampCamera(out clampX, out clampY);

        if (!tryingMove)
        {
            if (clampX)
            {
                targetPosition.x = transform.position.x;
            }
            if (clampY)
            {
                targetPosition.y = transform.position.y;
            }
        }
        cameraRect.x = transform.position.x;
        cameraRect.y = transform.position.y;
        updateViewRect();
        isMoved = true;
    }

    bool isMovedToTarget()
    {
        bool flag = Vector3.Distance(transform.position, targetPosition) <= 0.1f;
        if (flag)
        {
            targetPosition = transform.position;
        }
        return flag;
    }

    public void MovePosition(float x, float y)
    {
        if (x != 0 || y != 0)
        {
            transform.position += new Vector3(x, y);
            updateCameraRect();
        }
    }

    void beginZoom()
    {
        if (zoomState == 3)
        {
            zoomState = 4;
            zoomInvoke(BEGIN, isSnapZoom);
        }
    }

    void endZoom()
    {
        if (zoomState == 4)
        {
            zoomState = 5;
            zoomInvoke(END, isSnapZoom);
        }
    }

    void beginMove()
    {
        if (moveState == 0)
        {
            moveState = 1;
            moveInvoke(BEGIN, isSnapMove);
        }
    }

    void endMove()
    {
        if (moveState == 1)
        {
            moveState = 0;
            moveInvoke(END, isSnapMove);
            isSwipeMove = false;
            isSnapMove = false;
        }
    }

    bool touchZoom()
    {
        if (freezeZoom)
        {
            return false;
        }
        var fingers = LeanSelectable.GetFingers(IgnoreStartedOverGui, IgnoreIsOverGui, 2, null);
        var pinchRatio = LeanGesture.GetPinchRatio(fingers, ZoomSensitivity);

        float size = currentSize * pinchRatio;
        size = Mathf.Clamp(size, MinSize, MaxSize);
        if (size != currentSize)
        {
            if (zoomState == 0)
            {
                zoomState = 3;
            }
            currentSize = size;
            SetZoom();
            //BUG HERr
            endZoom();
            return true;
        }
        return false;
    }

    void SetZoom()
    {
        beginZoom();
        if (Camera)
        {
            Camera.orthographicSize = currentSize;
        }
        updateCameraRect();
    }

    public static float SineEaseOut(float t, float b, float c, float d)
    {
        return c * Mathf.Sin(t / d * (Mathf.PI / 2)) + b;
    }

    void LateUpdate()
    {
        //if (IgnoreTouch)
        //{
        //    return;
        //}
        isMoved = false;
        Vector3 beforeMove = transform.position;
        bool zoomed = touchZoom();
        if (!zoomed && !freezeMove)
        {
            if (isSnapMove)
            {
                beginMove();
                if(isSmoothMove)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, snapMoveMutiply);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * snapMoveMutiply);
                }
                updateCameraRect();
                if (isMovedToTarget())
                {
                    endMove();
                    isSnapMove = false;
                }
            }
            else if (isSwipeMove)
            {
                beginMove();
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
                updateCameraRect();
                if (isMovedToTarget())
                {
                    endMove();
                    isSwipeMove = false;
                }
            }
            else if (isFingerSet)
            {
                // isFingerDown = false;
                var fingers = LeanTouch.GetFingers(IgnoreStartedOverGui, IgnoreIsOverGui, 1);
                var lastScreenPoint = LeanGesture.GetLastScreenCenter(fingers);
                var screenPoint = LeanGesture.GetScreenCenter(fingers);
                var worldDelta = ScreenDepth.ConvertDelta(lastScreenPoint, screenPoint, Camera, gameObject);
                if(worldDelta != Vector3.zero){
                    beginMove();
                    transform.position -= worldDelta;
                    updateCameraRect();
                }
            }
        }

        if (isSnapZoom && !freezeZoom)
        {
            if (isSmoothZoom)
            {
                //snapZoomMutiply *= ((ZoomSteps+1) * (ZoomSteps + 1) / Mathf.Pow(Mathf.Abs(targetSize - currentSize)+1, 1));
                ZoomSteps += Time.deltaTime;
                //currentSize = Mathf.SmoothStep(currentSize, targetSize, Time.deltaTime * snapZoomMutiply);
                currentSize = SineEaseOut(ZoomSteps, ZoomStart, targetSize-ZoomStart, snapZoomMutiply);
            }
            else
            {
                currentSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * snapZoomMutiply);
            }
            SetZoom();
            if (Mathf.Abs(currentSize - targetSize) < 0.01f)
            {
                currentSize = targetSize;
                SetZoom();
                zoomed = true;
                endZoom();
                isSnapZoom = false;
            }
        }

        if (isMoved || zoomed)
        {
            actualMoveDelta = transform.position - beforeMove;
            if(scrollZoomEvent != null)
            {
                scrollZoomEvent.OnViewRectChange();
            }
        }
    }

    void updateViewRect()
    {
        ViewRect.width = cameraRect.width / maxRangeRect.width;
        ViewRect.height = cameraRect.height / maxRangeRect.height;
        ViewRect.x = (cameraRect.x - maxRangeRect.x) / (maxRangeRect.width * 2) + 0.5f;
        ViewRect.y = (cameraRect.y - maxRangeRect.y) / (maxRangeRect.height * 2) + 0.5f;
    }
}
