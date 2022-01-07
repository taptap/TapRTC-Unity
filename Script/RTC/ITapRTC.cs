using System.Threading.Tasks;

namespace TapTap.RTC
{
    public interface ITapRTC
    {
        Task<bool> Init(TapRTCConfig config);

        bool UnInit();

        bool Init(TapRTCConfig config, ProviderResponse response);

        ITapRTCAudioDevice GetAudioDevice();

        ITapRTCRoomDelegate AcquireRoom(string roomId, bool rangeAudio);
    
        void Poll();

        void Resume();

        void Pause();
    }
}