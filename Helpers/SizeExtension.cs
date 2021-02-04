using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Foundation;

namespace Windows.Graphics
{
    public static class SizeExtension
    {
        public static Size ToSizeF(this SizeInt32 size)
        {
            return new Size(size.Width, size.Height);
        }
    }
}
