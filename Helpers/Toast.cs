using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurePixels.Helpers
{
    public delegate void ToastHandle(string message, int duration, Action callBack);
    public class Toast
    {
        static ToastHandle OnToast { get; set; }

        public static void RegisterToastHandle(ToastHandle onToast) => OnToast = onToast;

        public static Task ShowAsync(string message, int timeout = 1500)
        {
            TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
            OnToast?.Invoke(message, timeout, () => source.SetResult(true));
            return source.Task;
        }
    }
}
