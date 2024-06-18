using System;

namespace Becs
{
    public abstract class Engine : IDisposable
    {
        public void Tick()
        {
            if (!IsTickable())
            {
                return;
            }

            TickEngine();
        }

        protected abstract bool IsTickable();
        protected abstract void TickEngine();

        public virtual void Dispose()
        {
        }
    }
}