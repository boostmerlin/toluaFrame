using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class VScrollList : LoopVerticalScrollRect, LoopScrollDataSource
{
    public System.Action<Transform, int> onItemRender;

    override protected void Awake()
    {
        base.Awake();
        dataSource = this;
        vertical = true;
        horizontal = false;
    }
    override protected void Start()
    {
        base.Start();
    }

    public System.Action<float> onScrollValue;
    protected override void OnEnable()
    {
        //base.onValueChanged.AddListener((v) =>
        //{
        //    if (onScrollValue != null)
        //    {
        //        onScrollValue(this.verticalNormalizedPosition);
        //    }
        //});
    }
    protected override void OnDisable()
    {
        //base.onValueChanged.RemoveAllListeners();
    }

    void LoopScrollDataSource.ProvideData(Transform transform, int idx)
    {
        if (onItemRender != null)
        {
            onItemRender(transform, idx);
        }
    }

    public bool Vertical
    {
        get
        {
            return base.vertical;
        }
        set
        {
            base.vertical = value;
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

    public void Clear()
    {
        this.ClearCells();
    }

    public void Refresh()
    {
        this.RefreshCells();
    }

    public void SetItemAndRender(GameObject gameObject)
    {
        if (prefabSource != null)
        {
            prefabSource.itemSrcObject = gameObject;
            RefillCells();
        }
    }

}
