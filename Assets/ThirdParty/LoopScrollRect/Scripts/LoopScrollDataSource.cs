using UnityEngine;
using System.Collections;
//modified by ml
namespace UnityEngine.UI
{
    public interface LoopScrollDataSource
    {
        void ProvideData(Transform transform, int idx);
    }

	public class LoopScrollSendIndexSource : LoopScrollDataSource
    {
		public static readonly LoopScrollSendIndexSource Instance = new LoopScrollSendIndexSource();

		LoopScrollSendIndexSource(){}

        public void ProvideData(Transform transform, int idx)
        {
            transform.SendMessage("ScrollCellIndex", idx, SendMessageOptions.DontRequireReceiver);
        }
    }

	public class LoopScrollArraySource<T> : LoopScrollDataSource
    {
        T[] objectsToFill;

		public LoopScrollArraySource(T[] objectsToFill)
        {
            this.objectsToFill = objectsToFill;
        }

        public void ProvideData(Transform transform, int idx)
        {
            transform.SendMessage("ScrollCellContent", objectsToFill[idx], SendMessageOptions.DontRequireReceiver);
        }
    }
}