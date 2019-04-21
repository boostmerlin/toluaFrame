namespace UnityEngine.UI
{
    [System.Serializable]
    public class LoopScrollPrefabSource 
    {
        public string itemPath;
        public GameObject itemSrcObject;
        public int poolSize = 5;

        private bool inited = false;

        public virtual GameObject GetObject()
        {
            if(!inited)
            {
                if(itemSrcObject != null)
                {
                    itemPath = itemSrcObject.name;
                }
                else if(!string.IsNullOrEmpty(itemPath))
                {
                    itemSrcObject = Resources.Load<GameObject>(itemPath);
                }
                else
                {
                    Debug.LogError("Invalid Loop Scroll Prefab Source Object.");
                }
                if(!itemSrcObject)
                {
                    Debug.LogError("No Loop Scroll Prefab Source Object.");
                }
                SG.ObjectPoolManager.Instance.InitPool(itemPath, poolSize, itemSrcObject);
                inited = true;
            }
            return SG.ObjectPoolManager.Instance.GetObjectFromPool(itemPath);
        }

        public virtual void ReturnObject(Transform go)
		{
            SG.ObjectPoolManager.Instance.ReturnObjectToPool(go.gameObject);
        }

		public virtual void ClearPool()
		{
			string key = "";
			if (itemSrcObject != null) 
			{
				key = itemSrcObject.name;
			} 
			else if (!string.IsNullOrEmpty (itemPath)) 
			{
				key = itemPath;
			}
			if (!string.IsNullOrEmpty(key))
				SG.ObjectPoolManager.Instance.ClearPool(key);
		}
    }
}
