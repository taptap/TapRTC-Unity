using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TencentMobileGaming;
using UnityEngine;
using UnityEngine.Networking;

namespace TapTap.RTC
{
    public abstract class ITapRTCRoomDelegate : ITapRTCEvent
    {
        protected readonly string RoomId;

        protected readonly TapRTCConfig Config;

        protected readonly bool RangeAudio;

        public HashSet<string> Users { get; }

        public readonly ConcurrentDictionary<string, string> TapIdMaps = new ConcurrentDictionary<string, string>();

        public readonly ConcurrentDictionary<string, string> OpenIdMaps = new ConcurrentDictionary<string, string>();

        private readonly HashSet<TapRTCEvent> actions = new HashSet<TapRTCEvent>();

        protected internal ITapRTCRoomDelegate(TapRTCConfig config, string roomId, bool rangeAudio)
        {
            Config = config;
            RoomId = roomId;
            RangeAudio = rangeAudio;
            Users = new HashSet<string>();
        }

        public abstract void Join(string authority);

        public abstract void Join(JoinRoomResponse response);

        public abstract void Exit();

        public abstract ITapRTCRangeAudioCtrl GetRtcRangeAudioCtrl();

        public abstract AudioPerfProfile ChangeRoomType(AudioPerfProfile profile);

        public abstract bool EnableAudioReceiver(bool enable);

        public abstract bool IsAudioReceiverEnabled();

        public abstract bool EnableUserAudio(string openId);

        public abstract bool DisableUserAudio(string openId);

        public void RegisterEventAction(TapRTCEvent action)
        {
            actions.Add(action);
        }

        public void RemoveEventAction(TapRTCEvent action)
        {
            actions.Remove(action);
        }

        public void ClearAction()
        {
            actions.Clear();
        }

        public void ForeachEventAction(ForeachRoomAction action)
        {
            foreach (var eventAction in actions)
            {
                action.Each(eventAction);
            }
        }

        protected async Task<JoinRoomResponse> GetRoomToken(string roomId, string authority)
        {
            return new JoinRoomResponse(await TapRTCHttpClient.GetInstance()
                .Get(TapRTCConstants.GET_AUTH_BUFF, null, new Dictionary<string, object>
                {
                    ["userId"] = Config.UserId,
                    ["deviceId"] = Config.DeviceId,
                    ["provider"] = Config.Provider.TransformToString(),
                    ["authBuffer"] = UnityWebRequest.EscapeURL(authority),
                    ["roomId"] = roomId
                }));
        }

        public string GetOpenId(string tapId)
        {
            return OpenIdMaps.ContainsKey(tapId) ? OpenIdMaps[tapId] : null;
        }

        protected async Task<string> FetchTapId(string openId)
        {
            if (TapIdMaps.ContainsKey(openId) && !string.IsNullOrEmpty(TapIdMaps[openId]))
            {
                return TapIdMaps[openId];
            }

            var id = await TapRTCHttpClient.GetInstance().Get(TapRTCConstants.TRANSFORM_CONVERT_USER, null,
                new Dictionary<string, object>
                {
                    ["userId"] = openId,
                    ["deviceId"] = Config.DeviceId,
                    ["provider"] = Config.Provider.TransformToString(),
                });

            TapIdMaps[openId] = id;
            OpenIdMaps[id] = openId;

            Debug.Log($"TapId:{Json.Serialize(TapIdMaps)}");
            Debug.Log($"OpenId:{Json.Serialize(OpenIdMaps)}");

            return id;
        }

        public virtual void OnEvent(EventType type, string data)
        {
        }
    }
}