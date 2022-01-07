namespace TapTap.RTC
{
    public interface ITapRTCAudioDevice
    {
        bool EnableSpatializer(bool enable, bool applyToTeam);

        bool EnableMic(bool enable);

        bool IsMicEnable();

        bool SetMicVolume(int value);

        int GetMicVolume();

        bool EnableAudioPlay(bool enable);

        bool IsAudioPlayEnabled();

        bool EnableSpeaker(bool enable);

        bool IsSpeakerEnabled();

        bool EnableLoopback(bool enable);

        bool SetSpeakerVolume(int volume);

        int GetSpeakerVolume();

        bool InitSpatializer();
    }
}