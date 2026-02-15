using System;

namespace ProjectileSim.Models
{
    public class SimulationState
    {
        public double Time { get; set; } = 0;
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public double VelocityX { get; set; } = 0;
        public double VelocityY { get; set; } = 0;
        public double Mass { get; set; } = 1.0;
        public double DragCoefficient { get; set; } = 0.47;
        public double Gravity { get; set; } = 9.81;
        public bool EnableDrag { get; set; } = false;

        public double CalculateVelocity() => Math.Sqrt(VelocityX * VelocityX + VelocityY * VelocityY);
        public double CalculateKineticEnergy() => 0.5 * Mass * CalculateVelocity() * CalculateVelocity();
        public double CalculatePotentialEnergy() => Mass * Gravity * Y;

        public void Step(double dt)
        {
            if (EnableDrag)
            {
                // Force drag = 0.5 * rho * Cd * A * v^2
                // Simplified: Fd = k * v^2 where k is a composite constant
                // Let's assume rho=1.225 (air), Area=0.1 (simplified sphere), Cd is user defined.
                double rho = 1.225;
                double Area = 0.05; // radius roughly 12cm
                double v = CalculateVelocity();
                
                double dragForce = 0.5 * rho * DragCoefficient * Area * v * v;
                double dragX = 0;
                double dragY = 0;

                if (v > 0)
                {
                    double angle = Math.Atan2(VelocityY, VelocityX);
                    dragX = dragForce * Math.Cos(angle);
                    dragY = dragForce * Math.Sin(angle);
                }

                double ax = -dragX / Mass;
                double ay = -Gravity - (dragY / Mass);

                VelocityX += ax * dt;
                VelocityY += ay * dt;
            }
            else
            {
                // Only gravity affects Y
                VelocityY -= Gravity * dt;
            }

            X += VelocityX * dt;
            Y += VelocityY * dt;
            Time += dt;

            if (Y < 0) // Hit ground
            {
                Y = 0;
                VelocityX = 0;
                VelocityY = 0;
            }
        }
    }
}