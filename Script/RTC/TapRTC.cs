using System.Threading.Tasks;

namespace TapTap.RTC
{
    public class TapRTC
    {
        public static Task<bool> Init(TapRTCConfig config)
        {
            return TapRTCDelegateImpl.GetInstance().Init(config);
        }

        public static bool UnInit()
        {
            return TapRTCDelegateImpl.GetInstance().UnInit();
        }

        public static ITapRTCAudioDevice GetAudioDevice()
        {
            return TapRTCDelegateImpl.GetInstance().GetAudioDevice();
        }

        public static ITapRTCRoomDelegate AcquireRoom(string roomId, bool rangeAudio = false)
        {
            return TapRTCDelegateImpl.GetInstance().AcquireRoom(roomId, rangeAudio);
        }

        public static void Poll()
        {
            TapRTCDelegateImpl.GetInstance().Poll();
        }

        public static void Resume()
        {
            TapRTCDelegateImpl.GetInstance().Resume();
        }


        public static void Pause()
        {
            TapRTCDelegateImpl.GetInstance().Pause();
        }

        public static string GetVersion()
        {
            return TapRTCConstants.VERISON;
        }
    }
}