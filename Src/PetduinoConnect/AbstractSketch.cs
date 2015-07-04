using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetduinoConnect
{
    public abstract class AbstractSketch : IDisposable
    {
        public abstract void Setup();
        public abstract void Loop();
        public virtual void Exit() { }

        public void Dispose()
        {
            Exit();
        }
    }
}
