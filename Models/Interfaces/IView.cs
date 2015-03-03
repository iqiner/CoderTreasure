using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.Interfaces
{
    public interface IView
    {
        void SetController(IController controller);
    }
}
