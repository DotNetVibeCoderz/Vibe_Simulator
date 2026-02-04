using Avalonia;
using Avalonia.Input;
using Avalonia.Media;

namespace PhysicsSimulator.Simulations
{
    public abstract class SimulationBase : ISimulation
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract void Update(double deltaTime, Size bounds);
        public abstract void Draw(DrawingContext context, Size bounds);

        public virtual void OnPointerPressed(PointerPoint point, Size bounds) { }
        public virtual void OnPointerMoved(PointerPoint point, Size bounds) { }
        public virtual void OnPointerReleased(PointerPoint point, Size bounds) { }
        public abstract void Reset();
    }
}