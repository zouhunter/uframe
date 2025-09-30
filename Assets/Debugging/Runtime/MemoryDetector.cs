using UnityEngine;
using UnityEngine.Profiling;

namespace UFrame.Debugging
{
    /// <summary>
    /// 内存检测器，目前只是输出Profiler信息
    /// </summary>
    [AddComponentMenu("UFrame/Debug/MemoryDetector")]
    public class MemoryDetector : MonoBehaviour
    {
        private readonly static string TotalAllocMemroyFormation = "Alloc Memory : {0}M";
        private readonly static string TotalReservedMemoryFormation = "Reserved Memory : {0}M";
        private readonly static string TotalUnusedReservedMemoryFormation = "Unused Reserved: {0}M";
        //private readonly static string RuntimeMemorySizeFormation = "RuntimeMemorySize: {0}M";
        private readonly static string MonoHeapFormation = "Mono Heap : {0}M";
        private readonly static string MonoUsedFormation = "Mono Used : {0}M";
        // 字节到兆
        private float ByteToM = 0.000001f;
        private Rect allocMemoryRect;
        private Rect reservedMemoryRect;
        private Rect unusedReservedMemoryRect;
        //private Rect RuntimeMemorySizeRect;
        private Rect monoHeapRect;
        private Rect monoUsedRect;

        [SerializeField]
        private int x = 5;
        [SerializeField]
        private int y = 60;
        [SerializeField]
        private int w = 1000;
        [SerializeField]
        private int h = 60;

        void ResetGUISize()
        {
            this.allocMemoryRect = new Rect(x, y, w, h);
            this.reservedMemoryRect = new Rect(x, y + h, w, h);
            this.unusedReservedMemoryRect = new Rect(x, y + 2 * h, w, h);
            //this.RuntimeMemorySizeRect = new Rect(x, y + 3 * h, w, h);
            this.monoHeapRect = new Rect(x, y + 3 * h, w, h);
            this.monoUsedRect = new Rect(x, y + 4 * h, w, h);
        }

        void OnGUI()
        {
            ResetGUISize();

            GUI.Label(this.allocMemoryRect,
                string.Format(TotalAllocMemroyFormation, Profiler.GetTotalAllocatedMemoryLong() * ByteToM));
            GUI.Label(this.reservedMemoryRect,
                string.Format(TotalReservedMemoryFormation, Profiler.GetTotalReservedMemoryLong() * ByteToM));
            GUI.Label(this.unusedReservedMemoryRect,
                string.Format(TotalUnusedReservedMemoryFormation, Profiler.GetTotalUnusedReservedMemoryLong() * ByteToM));

            GUI.Label(this.monoHeapRect,
                string.Format(MonoHeapFormation, Profiler.GetMonoHeapSizeLong() * ByteToM));
            GUI.Label(this.monoUsedRect,
                string.Format(MonoUsedFormation, Profiler.GetMonoUsedSizeLong() * ByteToM));
        }
    }
}