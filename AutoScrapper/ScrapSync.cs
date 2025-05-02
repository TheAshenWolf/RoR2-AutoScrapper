using System.Collections.Generic;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace AutoScrapper
{
    /// <summary>
    /// INetMessage which takes the items to remove from the client and
    /// sends them to the server.
    /// </summary>
    public class ScrapSync : INetMessage
    {
        /// <summary>
        /// This constructor is only used to register the message type.
        /// </summary>
        public ScrapSync()
        {
            _networkId = NetworkInstanceId.Invalid;
            _itemsToRemove = new Dictionary<ItemIndex, int>();
        }

        /// <summary>
        /// This is the proper constructor to use when sending the message.
        /// </summary>
        /// <param name="networkId">Network ID of the client</param>
        /// <param name="itemsToRemove">Dictionary of index:count pairs with information about items to remove</param>
        public ScrapSync(NetworkInstanceId networkId,
            Dictionary<ItemIndex, int> itemsToRemove)
        {
            _networkId = networkId;
            _itemsToRemove = itemsToRemove;
        }


        private NetworkInstanceId _networkId;
        private Dictionary<ItemIndex, int> _itemsToRemove;

        /// <inheritdoc/>
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(_networkId);

            writer.Write(_itemsToRemove.Count);
            foreach (KeyValuePair<ItemIndex, int> item in _itemsToRemove)
            {
                writer.Write(item.Key);
                writer.Write(item.Value);
            }
        }

        /// <inheritdoc/>
        public void Deserialize(NetworkReader reader)
        {
            _networkId = reader.ReadNetworkId();

            int count = reader.ReadInt32();
            _itemsToRemove = new Dictionary<ItemIndex, int>(count);
            for (int i = 0; i < count; i++)
            {
                ItemIndex key = reader.ReadItemIndex();
                int value = reader.ReadInt32();
                _itemsToRemove.Add(key, value);
            }
        }
        
        /// <summary>
        /// If the message is received on the client, we ignore it.
        /// On the server, we get the player determined by the Network ID.
        /// Based on the list of items to remove, we remove the items from the player's inventory
        /// and calculate the amount of scrap to give.
        ///
        /// Doing it this way we prevent the client from having authority about the items they receive.
        /// If they alter the packet to delete less items, they will receive less scrap.
        /// </summary>
        public void OnReceived()
        {
            if (!NetworkServer.active) return;

            // We need to get the player from the network ID
            GameObject player = NetworkServer.FindLocalObject(_networkId);
            CharacterBody localBody = player?.GetComponent<CharacterBody>();

            // Local body is the player's character
            if (localBody == null)
            {
                Debug.LogWarning("AutoScrapper: Local body is null. Cannot scrap items automatically.");
                return;
            }

            // Get the player's inventory
            Inventory inventory = localBody.inventory;
            if (inventory == null)
            {
                Debug.LogWarning("AutoScrapper: Local inventory is null. Cannot scrap items automatically.");
                return;
            }
            
            // The client only tells us what items to remove. This is the step where we determine 
            // how much scrap to give
            ScrapperReportCount report = new ScrapperReportCount();
            foreach ((ItemIndex itemId, int itemCount) in _itemsToRemove)
            {
                // Add it into the report struct
                report.Add(ItemCatalog.GetItemDef(itemId).tier, itemCount);

                // We have to go backwards, as Removing an item actually removes it from the array.
                // This is not exactly performance friendly, but it works.
                // Sadly, we cannot really change that.
                inventory.RemoveItem(itemId, itemCount);
            }

            // Add scrap
            if (report.white > 0)
                inventory.GiveItem(RoR2Content.Items.ScrapWhite.itemIndex, report.white);
            if (report.green > 0)
                inventory.GiveItem(RoR2Content.Items.ScrapGreen.itemIndex, report.green);
            if (report.red > 0)
                inventory.GiveItem(RoR2Content.Items.ScrapRed.itemIndex, report.red);
            if (report.yellow > 0)
                inventory.GiveItem(RoR2Content.Items.ScrapYellow.itemIndex, report.yellow);

            // Report the results to the chat
            Utility.ReportResults(localBody.GetUserName(), report);
        }
    }
}