using System;
using System.Collections.Generic;
using TencentMobileGaming;
using UnityEngine;

namespace TapTap.RTC.GMEImpl
{
    public class GMERTCRoomDelegateImpl : ITapRTCRoomDelegate, ITapRTCRangeAudioCtrl
    {
        private readonly ITMGContext context;

        public int TeamId { get; set; }

        public RangeAudioMode RangeAudioMode { get; set; }

        protected internal GMERTCRoomDelegateImpl(TapRTCConfig config, string roomId, bool rangeAudio) : base(config,
            roomId,
            rangeAudio)
        {
            context = ITMGContext.GetInstance();
        }

        public override async void Join(string authority)
        {
            Join(await GetRoomToken(RoomId, authority));
        }

        public override void Join(JoinRoomResponse response)
        {
            if (context.IsRoomEntered())
            {
                throw new TapRTCException(-1, "You already in room!");
            }

            var roomType = TransformToRoomType(Config.PerfProfile);
            var authBuffer = Convert.FromBase64String(response.AuthBuffer);
            var ret = context.EnterRoom(response.RoomId, roomType, authBuffer);
            if (ret != 0)
            {
                throw new TapRTCException(ret, "Enter GME room failure!");
            }
        }

        private static ITMGRoomType TransformToRoomType(AudioPerfProfile perfProfile)
        {
            switch (perfProfile)
            {
                case AudioPerfProfile.DEFAULT:
                    return ITMGRoomType.ITMG_ROOM_TYPE_STANDARD;
                case AudioPerfProfile.LOW:
                    return ITMGRoomType.ITMG_ROOM_TYPE_FLUENCY;
                case AudioPerfProfile.MID:
                    return ITMGRoomType.ITMG_ROOM_TYPE_STANDARD;
                default:
                    return ITMGRoomType.ITMG_ROOM_TYPE_HIGHQUALITY;
            }
        }

        private static AudioPerfProfile TransformToAudioProfile(int roomType)
        {
            switch (roomType)
            {
                case (int) ITMGRoomType.ITMG_ROOM_TYPE_STANDARD:
                    return AudioPerfProfile.MID;
                case (int) ITMGRoomType.ITMG_ROOM_TYPE_FLUENCY:
                    return AudioPerfProfile.LOW;
                default:
                    return AudioPerfProfile.HIGH;
            }
        }

        public override AudioPerfProfile ChangeRoomType(AudioPerfProfile profile)
        {
            var roomType = TransformToRoomType(profile);
            return TransformToAudioProfile(context.GetRoom().ChangeRoomType(roomType));
        }

        public override void Exit()
        {
            context.ExitRoom();
        }

        public override bool EnableAudioReceiver(bool enable)
        {
            return context.GetAudioCtrl().EnableAudioRecv(enable) == 0;
        }

        public override bool IsAudioReceiverEnabled()
        {
            return context.GetAudioCtrl().IsAudioRecvEnabled();
        }

        public override bool EnableUserAudio(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var openId = GetOpenId(id);

            if (string.IsNullOrEmpty(openId))
            {
                return false;
            }

            return context.GetAudioCtrl().RemoveAudioBlackList(openId) == 0;
        }

        public override bool DisableUserAudio(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var openId = GetOpenId(id);

            if (string.IsNullOrEmpty(openId))
            {
                return false;
            }

            return context.GetAudioCtrl().AddAudioBlackList(openId) == 0;
        }

        public bool SetRangeAudioMode(RangeAudioMode mode)
        {
            if (!RangeAudio)
            {
                throw new TapRTCException(-1, "This room did not enable range audio!");
            }

            bool result;
            if (mode == RangeAudioMode.TEAM)
            {
                result = context.SetRangeAudioMode(ITMGRangeAudioMode.ITMG_RANGE_AUDIO_MODE_TEAM) == QAVError.OK;
            }
            else
            {
                result = context.SetRangeAudioMode(ITMGRangeAudioMode.ITMG_RANGE_AUDIO_MODE_WORLD) == QAVError.OK;
            }

            if (!result) return false;
            RangeAudioMode = mode;
            return true;
        }

        public override ITapRTCRangeAudioCtrl GetRtcRangeAudioCtrl()
        {
            return this;
        }

        public bool SetRangeAudioTeam(int teamId)
        {
            if (!RangeAudio)
            {
                throw new TapRTCException(-1, "This room did not enable range audio!");
            }

            if (context.SetRangeAudioTeamID(teamId) != QAVError.OK) return false;
            TeamId = teamId;
            return true;
        }

        public bool UpdateAudioReceiverRange(int range)
        {
            if (!RangeAudio)
            {
                throw new TapRTCException(-1, "This room did not enable range audio!");
            }

            return context.GetRoom().UpdateAudioRecvRange(range) == QAVError.OK;
        }

        public bool UpdateSelfPosition(Position position, Forward forward)
        {
            if (!RangeAudio)
            {
                throw new TapRTCException(-1, "This room did not enable range audio!");
            }

            return context.GetRoom()
                .UpdateSelfPosition(new[] {position.x, position.y, position.z}, forward?.asixForward,
                    forward?.asixRight,
                    forward?.asixUp) == QAVError.OK;
        }

        public override async void OnEvent(EventType type, string data)
        {
            if (type == EventType.MAIN_EVENT_TYPE_USER_UPDATE)
            {
                var dic = Json.Deserialize(data) as Dictionary<string, object>;

                var eventID = Json.GetValue<int>(dic, "eventID");

                if (!(dic?["openIdList"] is List<object> openIdList)) return;

                foreach (var openId in openIdList)
                {
                    var tapId = await FetchTapId((string) openId);

                    if (string.IsNullOrEmpty(tapId)) return;

                    ForeachEventAction(new ForeachRoomAction
                    {
                        Each = @event =>
                        {
                            if (eventID == ITMGContext.EVENT_ID_ENDPOINT_ENTER)
                            {
                                //有成员进入房间
                                Users.Add(tapId);
                                @event.OnUserEnter(tapId);
                            }
                            else if (eventID == ITMGContext.EVENT_ID_ENDPOINT_EXIT)
                            {
                                //有成员退出房间
                                Users.Remove(tapId);
                                @event.OnUserExit(tapId);
                            }
                            else if (eventID == ITMGContext.EVENT_ID_ENDPOINT_HAS_AUDIO)
                            {
                                //有成员发送音频包
                                @event.OnUserSpeaker(tapId,
                                    ITMGContext.GetInstance().GetAudioCtrl().GetRecvStreamLevel((string) openId));
                            }
                            else if (eventID == ITMGContext.EVENT_ID_ENDPOINT_NO_AUDIO)
                            {
                                //有成员停止发送音频包
                                @event.OnUserSpeakEnd(tapId);
                            }
                        }
                    });
                }

                return;
            }

            if (type == EventType.MAIN_EVENT_TYPE_ENTER_ROOM)
            {
                ForeachEventAction(new ForeachRoomAction
                {
                    Each = @event =>
                    {
                        if (!(Json.Deserialize(data) is Dictionary<string, object> dataDic))
                        {
                            return;
                        }

                        var result = Json.GetValue<int>(dataDic, "result");
                        var errorInfo = Json.GetValue<string>(dataDic, "errorInfo");
                        if (result == 0)
                        {
                            @event.OnEnterSuccess();
                        }
                        else
                        {
                            @event.OnEnterFailure(errorInfo);
                        }
                    }
                });
            }
            else if (type == EventType.MAIN_EVENT_TYPE_EXIT_ROOM)
            {
                ForeachEventAction(new ForeachRoomAction
                {
                    Each = @event => { @event.OnExit(); }
                });
            }
            else if (type == EventType.MAIN_EVENT_TYPE_DISCONNECT)
            {
                ForeachEventAction(new ForeachRoomAction
                {
                    Each = @event =>
                    {
                        if (!(Json.Deserialize(data) is Dictionary<string, object> dataDic))
                        {
                            return;
                        }

                        var result = Json.GetValue<int>(dataDic, "result");
                        var errorInfo = Json.GetValue<string>(dataDic, "errorInfo");
                        @event.OnDisconnect(result, errorInfo);
                    }
                });
            }
            else if (type == EventType.MAIN_EVENT_TYPE_ROOM_TYPE_CHANGE)
            {
                ForeachEventAction(new ForeachRoomAction
                {
                    Each = @event =>
                    {
                        if (!(Json.Deserialize(data) is Dictionary<string, object> dataDic))
                        {
                            return;
                        }

                        var roomType = Json.GetValue<int>(dataDic, "roomType");
                        @event.OnRoomTypeChanged(roomType);
                    }
                });
            }
            else if (type == EventType.MAIN_EVENT_TYPE_CHANGE_ROOM_QUALITY)
            {
                ForeachEventAction(new ForeachRoomAction
                {
                    Each = @event =>
                    {
                        if (!(Json.Deserialize(data) is Dictionary<string, object> dataDic))
                        {
                            return;
                        }

                        var weight = Json.GetValue<int>(dataDic, "weight");
                        var loss = Json.GetValue<double>(dataDic, "loss");
                        var delay = Json.GetValue<int>(dataDic, "delay");
                        @event.OnRoomQualityChanged(weight, loss, delay);
                    }
                });
            }
        }
    }
}