using System.Numerics;

namespace PhysicsNet.Core
{
    public interface IScenario
    {
        string Name { get; }
        string Description { get; }
        void Setup(PhysicsWorld world);
        void Update(PhysicsWorld world, float dt); // Hook for frame-by-frame logic (input, custom gravity, etc)
        void OnInput(string key, bool isPressed); // Hook for input
    }
}