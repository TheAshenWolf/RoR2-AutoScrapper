using System.Collections.Generic;
using BepInEx.Configuration;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;

namespace AutoScrapper
{
    public class ConfigSync : INetMessage
    {
        private NetworkInstanceId _networkId;
        private Dictionary<string, bool> _generalSettings;
        private Dictionary<string, int> _itemSettings;

        private AutoScrapperConfig _config;

        public ConfigSync(NetworkInstanceId networkId, AutoScrapperConfig _config)
        {
            _networkId = networkId;

            if (NetworkClient.active)
            {
                _generalSettings = new Dictionary<string, bool>();
                _itemSettings = new Dictionary<string, int>();

                _generalSettings.Add("KeepScrapperClosed", _config.KeepScrapperClosed);
                _generalSettings.Add("ModEnabled", _config.ModEnabled);
                _generalSettings.Add("ReportEnabled", _config.ReportEnabled);

                foreach (KeyValuePair<ItemIndex, ConfigEntry<int>> entry in _config.configEntries)
                {
                    if (entry.Value.Value == -1) continue;
                    _itemSettings.Add(entry.Key.ToString(), entry.Value.Value);
                }
            }
        }

        /// <inheritdoc />
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(_networkId);
            writer.Write(_generalSettings.Count);
            foreach (KeyValuePair<string, bool> setting in _generalSettings)
            {
                writer.Write(setting.Key);
                writer.Write(setting.Value);
            }

            writer.Write(_itemSettings.Count);
            foreach (KeyValuePair<string, int> setting in _itemSettings)
            {
                writer.Write(setting.Key);
                writer.Write(setting.Value);
            }
        }

        /// <inheritdoc />
        public void Deserialize(NetworkReader reader)
        {
            _networkId = reader.ReadNetworkId();
            int generalSettingsCount = reader.ReadInt32();
            _generalSettings = new Dictionary<string, bool>(generalSettingsCount);
            for (int i = 0; i < generalSettingsCount; i++)
            {
                string key = reader.ReadString();
                bool value = reader.ReadBoolean();
                _generalSettings.Add(key, value);
            }

            int itemSettingsCount = reader.ReadInt32();
            _itemSettings = new Dictionary<string, int>(itemSettingsCount);
            for (int i = 0; i < itemSettingsCount; i++)
            {
                string key = reader.ReadString();
                int value = reader.ReadInt32();
                _itemSettings.Add(key, value);
            }
        }

        /// <inheritdoc />
        public void OnReceived()
        {
            if (NetworkClient.active)
                return;

            if (NetworkServer.active)
            {
                _config.clientGeneralConfigs[_networkId] = _generalSettings;
                _config.clientItemConfigs[_networkId] = _itemSettings;
            }
        }
    }
}