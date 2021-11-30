using System.Collections.Generic;
using System.Threading.Tasks;

namespace TapTap.RTC
{
    public class TapRTCDelegateImpl : ITapRTC
    {
        private static TapRTCDelegateImpl _sInstance;

        private static readonly object Locker = new object();

        private bool init = false;

        public static TapRTCDelegateImpl GetInstance()
        {
            lock (Locker)
            {
                if (_sInstance == null)
                {
                    _sInstance = new TapRTCDelegateImpl();
                }
            }

            return _sInstance;
        }

        public TapRTCConfig Config { get; set; }

        private ITapRTC rtcDelegate;

        public async Task<bool> Init(TapRTCConfig config)
        {
            if (init)
            {
                return init;
            }
            Config = config;
            rtcDelegate = ProviderFactory.Create(Config);
            TapRTCHttpClient.GetInstance().Init(Config);
            init = Init(config, await GetProviderConfig());
            return init;
        }

        public bool Init(TapRTCConfig config, ProviderResponse response)
        {
            return rtcDelegate.Init(config, response);
        }


        public bool UnInit()
        {
            init = false;
            TapRTCEventDispatcher.GetInstance().Clear();
            return rtcDelegate?.UnInit() == true;
        }

        public ITapRTCAudioDevice GetAudioDevice()
        {
            return rtcDelegate?.GetAudioDevice();
        }

        public ITapRTCRoomDelegate AcquireRoom(string roomId, bool rangeAudio)
        {
            return rtcDelegate?.AcquireRoom(roomId, rangeAudio);
        }

        public void Poll()
        {
            rtcDelegate?.Poll();
        }

        public void Resume()
        {
            rtcDelegate?.Resume();
        }

        public void Pause()
        {
            rtcDelegate?.Pause();
        }

        private async Task<ProviderResponse> GetProviderConfig()
        {
            return new ProviderResponse(await TapRTCHttpClient.GetInstance().Get(
                TapRTCConstants.GET_PROVIDER_CONFIG, null,
                new Dictionary<string, object>
                {
                    ["userId"] = Config.UserId,
                    ["deviceId"] = Config.DeviceId,
                    ["provider"] = Config.Provider.TransformToString()
                }));
        }
    }
}