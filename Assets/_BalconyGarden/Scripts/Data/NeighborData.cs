using System;
using System.Collections.Generic;
using UnityEngine;

namespace BalconyGarden
{
    [Serializable]
    public class NeighborStoryDialogue
    {
        public int requiredFriendship;
        [TextArea(2, 4)]
        public string dialogueText;
    }

    [CreateAssetMenu(fileName = "NewNeighbor", menuName = "Balcony Garden/Neighbor Data")]
    public class NeighborData : ScriptableObject
    {
        public string neighborId = "ba_lan";
        public string neighborName = "Bà Lan";
        public Sprite avatarSprite;
        [TextArea] public string bio = "Bà hàng xóm hiền lành, thích tự nấu ăn và ngắm hoa ban công.";
        public List<string> favoritePlantIds = new List<string>();
        public List<NeighborStoryDialogue> storyDialogues = new List<NeighborStoryDialogue>();
    }

    [Serializable]
    public class NeighborActiveOrder
    {
        public string neighborId;
        public string requiredPlantId;
        public int requiredAmount;
        public int rewardCoins;
        public int rewardXP;
        public int rewardFriendship;
        public bool isCompleted;
    }
}
