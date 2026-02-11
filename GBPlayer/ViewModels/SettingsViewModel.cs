using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Avalonia.Input;
using GBPlayer.Core;
using GBPlayer.Models;
using ReactiveUI;

namespace GBPlayer.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private AppSettings _currentSettings;

        public AppSettings CurrentSettings => _currentSettings;

        public int TargetFps
        {
            get => _currentSettings.TargetFps;
            set
            {
                if (_currentSettings.TargetFps != value)
                {
                    _currentSettings.TargetFps = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public IReadOnlyList<Key> AvailableKeys { get; }

        public Key UpKey
        {
            get => GetKey(GameBoyKey.Up);
            set => SetKey(GameBoyKey.Up, value, nameof(UpKey));
        }

        public Key DownKey
        {
            get => GetKey(GameBoyKey.Down);
            set => SetKey(GameBoyKey.Down, value, nameof(DownKey));
        }

        public Key LeftKey
        {
            get => GetKey(GameBoyKey.Left);
            set => SetKey(GameBoyKey.Left, value, nameof(LeftKey));
        }

        public Key RightKey
        {
            get => GetKey(GameBoyKey.Right);
            set => SetKey(GameBoyKey.Right, value, nameof(RightKey));
        }

        public Key AKey
        {
            get => GetKey(GameBoyKey.A);
            set => SetKey(GameBoyKey.A, value, nameof(AKey));
        }

        public Key BKey
        {
            get => GetKey(GameBoyKey.B);
            set => SetKey(GameBoyKey.B, value, nameof(BKey));
        }

        public Key SelectKey
        {
            get => GetKey(GameBoyKey.Select);
            set => SetKey(GameBoyKey.Select, value, nameof(SelectKey));
        }

        public Key StartKey
        {
            get => GetKey(GameBoyKey.Start);
            set => SetKey(GameBoyKey.Start, value, nameof(StartKey));
        }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public event Action<AppSettings> OnSaveRequested;
        public event Action OnCancelRequested;

        public SettingsViewModel()
            : this(new AppSettings())
        {
        }

        public SettingsViewModel(AppSettings existingSettings)
        {
            // Clone settings to avoid modifying the original until saved
            _currentSettings = new AppSettings
            {
                TargetFps = existingSettings.TargetFps,
                KeyMapping = new Dictionary<GameBoyKey, Key>(existingSettings.KeyMapping)
            };

            AvailableKeys = BuildAvailableKeys();

            SaveCommand = ReactiveCommand.Create(() => OnSaveRequested?.Invoke(_currentSettings));
            CancelCommand = ReactiveCommand.Create(() => OnCancelRequested?.Invoke());
        }

        private IReadOnlyList<Key> BuildAvailableKeys()
        {
            var commonKeys = new List<Key>
            {
                Key.Up, Key.Down, Key.Left, Key.Right,
                Key.Z, Key.X, Key.A, Key.S, Key.D, Key.W, Key.E, Key.R,
                Key.Space, Key.Enter, Key.Tab,
                Key.LeftShift, Key.RightShift,
                Key.LeftCtrl, Key.RightCtrl,
                Key.LeftAlt, Key.RightAlt
            };

            return commonKeys.Distinct().ToList();
        }

        private Key GetKey(GameBoyKey key)
        {
            return _currentSettings.KeyMapping.TryGetValue(key, out var mappedKey) ? mappedKey : Key.None;
        }

        private void SetKey(GameBoyKey key, Key value, string propertyName)
        {
            if (_currentSettings.KeyMapping.TryGetValue(key, out var existing) && existing == value)
            {
                return;
            }

            _currentSettings.KeyMapping[key] = value;
            this.RaisePropertyChanged(propertyName);
        }
    }
}
