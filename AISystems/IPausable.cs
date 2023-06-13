using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISystems.BT
{
    public interface IPausable
    {
        public bool isPaused { get; set; }
        public void Pause(bool pause);
    }
}
