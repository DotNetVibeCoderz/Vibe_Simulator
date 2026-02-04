using Avalonia;
using Avalonia.Media;
using Avalonia.Input;

namespace PhysicsSimulator.Simulations
{
    public interface ISimulation
    {
        string Name { get; }
        string Description { get; }
        void Update(double deltaTime, Size bounds);
        void Draw(DrawingContext context, Size bounds);
        void OnPointerPressed(PointerPoint point, Size bounds);
        void OnPointerMoved(PointerPoint point, Size bounds);
        void OnPointerReleased(PointerPoint point, Size bounds);
        void Reset();
    }
}