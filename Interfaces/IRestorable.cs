using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFabric.Serialization.V2.Interfaces
{
    /// <summary>
    /// Restores the item after serialization
    /// </summary>
    public interface IRestorable
    {
        void Restore();
    }
}
