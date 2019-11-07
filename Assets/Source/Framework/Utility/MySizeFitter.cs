using UnityEngine;

public class MySizeFitter : MonoBehaviour
{
#if UNITY_EDITOR
    void Reset()
    {
        MergeRect();
    }

    void OnValidate()
    {
        MergeRect();
    }
#endif

    public void MergeRect()
    {
        MergeRect((RectTransform)transform, anchor);
    }

    public enum AnchorRef
    {
        LeftTop = 0,
        LeftBottom = 1,
        RightTop = 2,
        RightBottom = 3
    }

    public Vector2 MergeRect(RectTransform self, AnchorRef anchor)
    {
        switch (anchor)
        {
            case AnchorRef.LeftTop:
                return GetLeftTopAndRightBottom(self, true);
            case AnchorRef.LeftBottom:
                return GetLeftBottomAndRightTop(self, true);
            case AnchorRef.RightTop:
                return GetLeftBottomAndRightTop(self, false);
            case AnchorRef.RightBottom:
                return GetLeftTopAndRightBottom(self, false);
        }
        return Vector2.zero;
    }

    public static Vector3 GetVertexPos(RectTransform rTra, float vertexX, float vertexY)
    {
        return GetVertexPos(rTra, new Vector2(vertexX, vertexY));
    }

    public static Vector3 GetVertexPos(RectTransform rTra, Vector2 vertex)
    {
        Vector2 delta = rTra.pivot - vertex;
        float x = delta.x * rTra.sizeDelta.x;
        float y = delta.y * rTra.sizeDelta.y;
        return rTra.localPosition - new Vector3(x, y, 0);
    }

    private static Vector2 GetLeftTopAndRightBottom(RectTransform self, bool setToLeftTop)
    {
        if (self == null)
            return Vector2.zero;

        Vector2 rightBottom = Vector2.zero;
        Vector2 leftTop = Vector2.zero;
        foreach (RectTransform child in self)
        {
            if (!child.gameObject.activeSelf || child == self)
                continue;

            Vector2 cLT = GetVertexPos(child, Vector2.up);
            Vector2 cRB = GetVertexPos(child, Vector2.right);

            if (rightBottom == leftTop)
            {
                leftTop = cLT;
                rightBottom = cRB;
            }
            else
            {
                if (cRB.x > rightBottom.x)
                    rightBottom.x = cRB.x;
                if (cRB.y < rightBottom.y)
                    rightBottom.y = cRB.y;
                if (cLT.x < leftTop.x)
                    leftTop.x = cLT.x;
                if (cLT.y > leftTop.y)
                    leftTop.y = cLT.y;
            }
        }

        return UpdateSelfLeftTop(self, leftTop, rightBottom, setToLeftTop);
    }

    private static Vector2 UpdateSelfLeftTop(RectTransform self, Vector2 leftTop, Vector2 rightBottom, bool setLeftTop)
    {
        Vector2 dir = setLeftTop ? Vector2.up : Vector2.right;
        Vector3 vertex = GetVertexPos(self, dir);
        Vector3 delta = (Vector3)(setLeftTop ? leftTop : rightBottom);
        Vector3[] list = TranslateChildren(self, vertex, delta);
        Vector2 pos = self.anchoredPosition;
        Vector2 size = Vector2.zero;

        size.x = rightBottom.x - leftTop.x;
        size.y = leftTop.y - rightBottom.y;
        self.sizeDelta = size;
        self.anchoredPosition = pos;

        Vector3 newVertex = GetVertexPos(self, dir);
        TranslateChildren(self, list, vertex, newVertex);
        return size;
    }

    private static Vector2 GetLeftBottomAndRightTop(RectTransform self, bool setLeftBottom)
    {
        if (self == null)
            return Vector2.zero;

        Vector2 leftBottom = Vector2.zero;
        Vector2 rightTop = Vector2.zero;
        foreach (RectTransform child in self)
        {
            if (!child.gameObject.activeSelf || child == self)
                continue;

            Vector2 cLB = GetVertexPos(child, Vector2.zero);
            Vector2 cRT = GetVertexPos(child, Vector2.one);

            if (leftBottom == rightTop)
            {
                leftBottom = cLB;
                rightTop = cRT;
            }
            else
            {
                if (cLB.x < leftBottom.x)
                    leftBottom.x = cLB.x;
                if (cLB.y < leftBottom.y)
                    leftBottom.y = cLB.y;
                if (cRT.x > rightTop.x)
                    rightTop.x = cRT.x;
                if (cRT.y > rightTop.y)
                    rightTop.y = cRT.y;
            }
        }

        return UpdateSelfLeftBottom(self, leftBottom, rightTop, setLeftBottom);
    }

    private static Vector2 UpdateSelfLeftBottom(RectTransform self, Vector2 leftBottom, Vector2 rightTop, bool setLeftBottom)
    {
        Vector2 dir = setLeftBottom ? Vector2.zero : Vector2.one;
        Vector3 vertex = GetVertexPos(self, dir);
        Vector3 delta = (Vector3)(setLeftBottom ? leftBottom : rightTop);
        Vector3[] list = TranslateChildren(self, vertex, delta);
        Vector2 pos = self.anchoredPosition;

        self.sizeDelta = rightTop - leftBottom;
        self.anchoredPosition = pos;

        Vector3 newVertex = GetVertexPos(self, dir);
        TranslateChildren(self, list, vertex, newVertex);
        return self.sizeDelta;
    }

    private static Vector3[] TranslateChildren(RectTransform self, Vector3 vertex, Vector3 delta)
    {
        Vector3 cPos = self.localPosition;
        Vector3 delta1 = vertex - cPos - delta;
        Vector3[] list = new Vector3[self.childCount];

        for (int i = 0; i < self.childCount; ++i)
        {
            Transform child = self.GetChild(i);
            child.localPosition += delta1;
            list[i] = child.localPosition;
        }
        return list;
    }

    private static void TranslateChildren(RectTransform self, Vector3[] list, Vector3 vertex, Vector3 newVertex)
    {
        Vector3 delta = newVertex - vertex;
        for (int i = 0; i < self.childCount; ++i)
            self.GetChild(i).localPosition = list[i] + delta;
        self.localPosition -= delta;
    }

    public AnchorRef anchor;

    void Awake()
    {
        MergeRect();
    }


}
