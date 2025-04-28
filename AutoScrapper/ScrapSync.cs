using System.Collections.Generic;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace AutoScrapper;

public class ScrapSync(
    NetworkInstanceId networkId,
    ScrapperReportCount reportCount,
    Dictionary<ItemIndex, int> itemsToRemove)
    : INetMessage
{
    public NetworkInstanceId NetworkId { get; private set; } = networkId;
    public ScrapperReportCount ReportCount { get; private set; } = reportCount;
    public Dictionary<ItemIndex, int> ItemsToRemove { get; private set; } = itemsToRemove;

    public void Serialize(NetworkWriter writer)
    {
        writer.Write(NetworkId);
        writer.Write(ReportCount.white);
        writer.Write(ReportCount.green);
        writer.Write(ReportCount.red);
        writer.Write(ReportCount.yellow);

        writer.Write(ItemsToRemove.Count);
        foreach (KeyValuePair<ItemIndex, int> item in ItemsToRemove)
        {
            writer.Write(item.Key);
            writer.Write(item.Value);
        }
    }

    public void Deserialize(NetworkReader reader)
    {
        NetworkId = reader.ReadNetworkId();
        ReportCount = new ScrapperReportCount()
        {
            white = reader.ReadInt32(),
            green = reader.ReadInt32(),
            red = reader.ReadInt32(),
            yellow = reader.ReadInt32()
        };

        int count = reader.ReadInt32();
        ItemsToRemove = new Dictionary<ItemIndex, int>(count);
        for (int i = 0; i < count; i++)
        {
            ItemIndex key = reader.ReadItemIndex();
            int value = reader.ReadInt32();
            ItemsToRemove.Add(key, value);
        }
    }

    public void OnReceived()
    {
        if (!NetworkServer.active) return;

        // We need to get the player from the network ID
        GameObject player = NetworkServer.FindLocalObject(NetworkId);
        CharacterBody localBody = player?.GetComponent<CharacterBody>();

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

        foreach (KeyValuePair<ItemIndex, int> pair in ItemsToRemove)
        {
            ItemIndex itemId = pair.Key;
            int itemCount = pair.Value;

            // We have to go backwards, as Removing an item actually removes it from the array.
            // This is not exactly performance friendly, but it works.
            // Sadly, we cannot really change that.
            inventory.RemoveItem(itemId, itemCount);
        }
        
        // Add scrap
        if (ReportCount.white > 0)
            inventory.GiveItem(RoR2Content.Items.ScrapWhite.itemIndex, ReportCount.white);
        if (ReportCount.green > 0)
            inventory.GiveItem(RoR2Content.Items.ScrapGreen.itemIndex, ReportCount.green);
        if (ReportCount.red > 0)
            inventory.GiveItem(RoR2Content.Items.ScrapRed.itemIndex, ReportCount.red);
        if (ReportCount.yellow > 0)
            inventory.GiveItem(RoR2Content.Items.ScrapYellow.itemIndex, ReportCount.yellow);
        
        // Report the results to the chat
        ReportResults(localBody.GetUserName(), ReportCount);
    }
    
    /// <summary>
    /// Reports the results of scrapping into the chat window.
    /// </summary>
    private void ReportResults(string userName, ScrapperReportCount count)
    {
        List<string> parts = count.GetReportParts();
            
        int partsCount = parts.Count;
        if (partsCount == 0)
            return;
            
        string result = "<color=#2083fc>" + userName + "</color> <color=#DDDDDD>" + Language.GetString("AUTO_SCRAPPER_AUTOMAGICALLY_SCRAPPED") + " ";
            
        if (partsCount == 1)
            result += parts[0] + ".";
        else if (partsCount == 2)
            result += parts[0] + " " + Language.GetString("AUTO_SCRAPPER_AND") + " " + parts[1] + ".";
        else if (partsCount > 2)
        {
            for (int i = 0; i < partsCount; i++)
            {
                if (i > 0)
                {
                    if (i == partsCount - 1)
                        result += ", " + Language.GetString("AUTO_SCRAPPER_AND") + " ";
                    else
                        result += ", ";
                }
                result += parts[i];
            }
            result += ".";
        }
        result += "</color>";

        Chat.SimpleChatMessage chat = new Chat.SimpleChatMessage();
        chat.baseToken = result;
            
        Chat.SendBroadcastChat(chat);
    }
}