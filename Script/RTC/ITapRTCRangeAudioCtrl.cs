namespace TapTap.RTC
{
    public interface ITapRTCRangeAudioCtrl
    {
        bool SetRangeAudioMode(RangeAudioMode mode);

        bool SetRangeAudioTeam(int teamId);

        bool UpdateAudioReceiverRange(int range);

        bool UpdateSelfPosition(Position position, Forward forward);
    }

    public class Position
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class Forward
    {
        public float[] asixForward = new float[3];

        public float[] asixRight = new float[3];

        public float[] asixUp = new float[3];
    }

    public enum RangeAudioMode
    {
        TEAM,
        WORLD
    }
}