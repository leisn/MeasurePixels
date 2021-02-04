using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurePixels.ViewModels
{
    public class EditorViewModel : BaseViewModel
    {
        bool isClipboardChanged = false;

        public bool IsClipboardBitmapChanged
        {
            get => isClipboardChanged;
            set => SetProperty(ref isClipboardChanged, value);
        }

        bool isCanvasHasItem = false;

        public bool IsCanvasHasItem
        {
            get => isCanvasHasItem;
            set => SetProperty(ref isCanvasHasItem, value);
        }
    }
}
