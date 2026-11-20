using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace First_Playable_Roman.Scripts
{
    public class PlayerSettings
    {
        public int Health { get; set; } = 100;
        public int Speed { get; set; } = 1;
    }

    public class LurkingSettings
    {
        public int Health { get; set; } = 50;
        public int Speed { get; set; } = 5;
    }

    public class ChaserSettings
    {
        public int Health { get; set; } = 75;
        public int Speed { get; set; } = 3;
        public float DetectionRadius { get; set; } = 200f;
    }

    public class TurretSettings
    {
        public int Health { get; set; } = 9999;
    }

    public class EnemySettings
    {
        public LurkingSettings Lurking { get; set; } = new LurkingSettings();
        public ChaserSettings Chaser { get; set; } = new ChaserSettings();
        public TurretSettings Turret { get; set; } = new TurretSettings();
    }

    public class BowSettings
    {
        public float ChargeTime { get; set; } = 1.0f;
    }

    // Root object representing the game's configurable settings (GameSettings.json)
    public class GameSettingsData
    {
        public PlayerSettings Player { get; set; } = new PlayerSettings();
        public EnemySettings Enemies { get; set; } = new EnemySettings();
        public BowSettings Bow { get; set; } = new BowSettings();
    }

    // Static accessor that loads GameSettings.json once and exposes the values
    // used throughout the game (player/enemy health & speed, bow charge speed, etc.).
    public static class GameSettings
    {
        private const string FileName = "GameSettings.json";

        private static GameSettingsData _data;

        public static GameSettingsData Data
        {
            get
            {
                if (_data == null)
                    Load();

                return _data;
            }
        }

        public static void Load()
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, FileName);

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    _data = JsonSerializer.Deserialize<GameSettingsData>(json, options) ?? new GameSettingsData();
                }
                else
                {
                    _data = new GameSettingsData();
                }
            }
            catch (Exception)
            {
                // Fall back to default values if the file is missing or malformed
                _data = new GameSettingsData();
            }
        }
    }
}
