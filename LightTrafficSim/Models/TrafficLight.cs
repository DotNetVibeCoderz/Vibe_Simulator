namespace LightTrafficSim.Models
{
    public enum TrafficLightState
    {
        Green,
        Yellow,
        Red
    }

    public class TrafficLight
    {
        public TrafficLightState State { get; set; } = TrafficLightState.Red;
    }
}