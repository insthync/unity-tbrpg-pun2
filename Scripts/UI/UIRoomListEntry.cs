﻿using Colyseus;
using RealtimeArena.Room;
using UnityEngine.UI;

namespace RealtimeArena.UI
{
    public class UIRoomListEntry : UIBase
    {
        public UIRoomList uiRoomList;
        public Text textTitle;

        public string RoomId { get; set; }
        public bool HasPassword { get; set; }

        private string _roomTitle;
        public string RoomTitle
        {
            get { return _roomTitle; }
            set
            {
                _roomTitle = value;
                if (textTitle)
                    textTitle.text = value;
            }
        }

        public void OnClickJoin()
        {
            // Show room password UI if the room password is required
            if (HasPassword)
                uiRoomList.ShowUIRoomPassword(RoomId, RoomTitle);
            else
                RealtimeArenaManager.Instance.JoinRoom(RoomId, null);
        }
    }
}
