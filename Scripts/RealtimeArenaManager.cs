﻿using UnityEngine;
using Colyseus;
using RealtimeArena.Room;
using RealtimeArena.Event;
using UnityEngine.Events;
using System.Collections.Generic;
using RealtimeArena.Message;

namespace RealtimeArena
{
    public class RealtimeArenaManager : MonoBehaviour
    {
        public static RealtimeArenaManager Instance { get; private set; }
        public static ColyseusClient Client { get; private set; }
        public static ColyseusRoom<GameRoomState> CurrentRoom { get; set; }
        public static bool IsManager { get { return CurrentRoom != null && CurrentRoom.SessionId == CurrentRoom.State.managerSessionId; } }

        public string serverAddress = "ws://localhost:2567";
        public string battleScene = "OnlineBattleScene";
        public UnityEvent onJoinRoom = new UnityEvent();
        public StringEvent onJoinRoomFailed = new StringEvent();
        public RoomErrorEvent onRoomError = new RoomErrorEvent();
        public RoomLeaveEvent onLeaveRoom = new RoomLeaveEvent();
        public StringEvent onPlayerLeave = new StringEvent();
        public RoomStateChangeEvent onRoomStateChange = new RoomStateChangeEvent();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Client = new ColyseusClient(serverAddress);
        }

        private void OnApplicationQuit()
        {
            LeaveFromTheRoom();
        }

        public async void LeaveFromTheRoom()
        {
            if (CurrentRoom != null)
                await CurrentRoom.Leave(true);
        }

        public async void CreateRoom(Dictionary<string, object> options)
        {
            if (options == null)
                options = new Dictionary<string, object>();
            options[GameRoomConsts.OPTION_PLAYER_ID] = Player.CurrentPlayer.Id;
            options[GameRoomConsts.OPTION_LOGIN_TOKEN] = Player.CurrentPlayer.LoginToken;
            try
            {
                ColyseusRoom<GameRoomState> room = await Client.Create<GameRoomState>(GameRoomConsts.ROOM_NAME, options);
                OnJoinRoom(room);
            }
            catch (System.Exception ex)
            {
                OnJoinRoomFailed(ex.Message);
            }
        }

        public async void JoinRoom(string roomId, Dictionary<string, object> options)
        {
            if (options == null)
                options = new Dictionary<string, object>();
            options[GameRoomConsts.OPTION_PLAYER_ID] = Player.CurrentPlayer.Id;
            options[GameRoomConsts.OPTION_LOGIN_TOKEN] = Player.CurrentPlayer.LoginToken;
            try
            {
                ColyseusRoom<GameRoomState> room = await Client.JoinById<GameRoomState>(roomId, options);
                OnJoinRoom(room);
            }
            catch (System.Exception ex)
            {
                OnJoinRoomFailed(ex.Message);
            }
        }

        private void OnJoinRoom(ColyseusRoom<GameRoomState> room)
        {
            CurrentRoom = room;
            CurrentRoom.OnError += CurrentRoom_OnError;
            CurrentRoom.OnStateChange += CurrentRoom_OnStateChange;
            CurrentRoom.OnLeave += CurrentRoom_OnLeave;
            CurrentRoom.OnMessage<string>("playerLeave", CurrentRoom_OnPlayerLeave);
            onJoinRoom.Invoke();
        }

        private void OnJoinRoomFailed(string message)
        {
            Debug.LogError($"Join Lobby Failed: {message}");
            onJoinRoomFailed.Invoke(message);
        }

        private void CurrentRoom_OnError(int code, string message)
        {
            onRoomError.Invoke(code, message);
        }

        private void CurrentRoom_OnStateChange(GameRoomState state, bool isFirstState)
        {
            onRoomStateChange.Invoke(state, isFirstState);
        }

        private void CurrentRoom_OnLeave(int code)
        {
            onLeaveRoom.Invoke(code);
            CurrentRoom = null;
        }

        private void CurrentRoom_OnPlayerLeave(string sessionId)
        {
            onPlayerLeave.Invoke(sessionId);
        }

        public void LoadBattleScene(bool loadIfNotLoaded = false)
        {
            GameInstance.Singleton.LoadSceneIfNotLoaded(battleScene, loadIfNotLoaded);
        }

        public async void TogglePlayerReadyState()
        {
            await CurrentRoom.Send("ready");
        }

        public async void SetPlayerEnterGameState()
        {
            await CurrentRoom.Send("enterGame");
        }

        public async void SendUpdateActiveCharacter(string id)
        {
            await CurrentRoom.Send("updateActiveCharacter", id);
        }

        public void SendDoSelectedAction(string entityId, string targetEntityId, int action, int seed)
        {
            SendDoSelectedAction(new DoSelectedActionMsg()
            {
                entityId = entityId,
                targetEntityId = targetEntityId,
                action = action,
                seed = seed,
            });
        }

        public async void SendDoSelectedAction(DoSelectedActionMsg msg)
        {
            await CurrentRoom.Send("doSelectedAction", msg);
        }

        public async void SendUpdateGameplayState(UpdateGameplayStateMsg msg)
        {
            await CurrentRoom.Send("updateGameplayState", msg);
        }
    }
}
