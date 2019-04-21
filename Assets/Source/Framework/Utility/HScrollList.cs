using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HScrollList : LoopHorizontalScrollRect, LoopScrollDataSource
{
    public System.Action<Transform, int> onItemRender;


    override protected void Awake()
    {
        base.Awake();
        dataSource = this;
        vertical = false;
        horizontal = true;
    }
    override protected void Start()
    {
        base.Start();
    }
    public System.Action<float> onScrollValue;

    int lastState = -1;

    protected override void OnEnable()
    {
        base.onValueChanged.AddListener((v) =>
        {
            int state = -1;
            if (v.x <= 0)
            {
                state = 0;
            }
            else if (v.x >= 1 && v.x < 2)
            {
                state = 1;
            }
            else if(v.x > 1)
            {
                state = 2;
            }
            if(lastState != state)
            {
                if (onScrollValue != null)
                {
                    onScrollValue(state);
                }
                lastState = state;
            }
        });
    }
    protected override void OnDisable()
    {
        base.onValueChanged.RemoveAllListeners();
    }

    public void Clear()
    {
        this.ClearCells();
    }

    public void Refresh()
    {
        this.RefreshCells();
    }

    public bool Horizontal
    {
        get
        {
            return base.horizontal;
        }
        set
        {
            base.horizontal = value;
        }
    }

    void LoopScrollDataSource.ProvideData(Transform transform, int idx)
    {
        if (onItemRender != null)
        {
            onItemRender(transform, idx);
        }
    }

    public int DataCount
    {
        set
        {
            totalCount = value;
            RefillCells();
        }
    }

    public void SetItemAndRender(GameObject gameObject)
    {
        if(prefabSource != null)
        {
            prefabSource.itemSrcObject = gameObject;
            RefillCells();
        }
    }
}
