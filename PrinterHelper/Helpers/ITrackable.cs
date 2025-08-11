using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrinterHelper.Helpers
{
    public interface ITrackable
    {
        bool HasChanged();
        void Solidify();
    }
}
