using System.Collections.Generic;
using Avalonia.Input;
using GBPlayer.Core;

namespace GBPlayer.Models
{
    public class AppSettings
    {
        public int TargetFps { get; set; } = 60;
        public Dictionary<GameBoyKey, Key> KeyMapping { get; set; }

        public AppSettings()
        {
            // Default mappings
            KeyMapping = new Dictionary<GameBoyKey, Key>
            {
                { GameBoyKey.Up, Key.Up },
                { GameBoyKey.Down, Key.Down },
                { GameBoyKey.Left, Key.Left },
                { GameBoyKey.Right, Key.Right },
                { GameBoyKey.A, Key.Z },
                { GameBoyKey.B, Key.X },
                { GameBoyKey.Select, Key.Space },
                { GameBoyKey.Start, Key.Enter }
            };
        }
    }
}