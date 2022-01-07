using System;
using System.IO;
using TencentMobileGaming;
using UnityEngine;

namespace TapTap.RTC.GMEImpl
{
    public class GMERTCAudioDeviceImpl : ITapRTCAudioDevice
    {
        private ITMGContext _context;

        public GMERTCAudioDeviceImpl(ITMGContext context)
        {
            _context = context;
        }

        public bool EnableMic(bool enable)
        {
            return _context.GetAudioCtrl().EnableMic(enable) == 0;
        }

        public bool IsMicEnable()
        {
            return _context.GetAudioCtrl().GetMicState() == 0;
        }

        public bool SetMicVolume(int value)
        {
            return _context.GetAudioCtrl().SetMicVolume(value) == 0;
        }

        public int GetMicVolume()
        {
            return _context.GetAudioCtrl().GetMicVolume();
        }

        public bool EnableAudioPlay(bool enable)
        {
            return _context.GetAudioCtrl().EnableAudioPlayDevice(enable) == 0;
        }

        public bool IsAudioPlayEnabled()
        {
            return _context.GetAudioCtrl().IsAudioPlayDeviceEnabled();
        }

        public bool EnableSpeaker(bool enable)
        {
            return _context.GetAudioCtrl().EnableSpeaker(enable) == 0;
        }

        public bool IsSpeakerEnabled()
        {
            return _context.GetAudioCtrl().IsEnableSpatializer();
        }

        public bool EnableLoopback(bool enable)
        {
            return _context.GetAudioCtrl().EnableLoopBack(enable) == 0;
        }

        public bool SetSpeakerVolume(int volume)
        {
            return _context.GetAudioCtrl().SetSpeakerVolume(volume) == 0;
        }

        public int GetSpeakerVolume()
        {
            return _context.GetAudioCtrl().GetSpeakerVolume();
        }

        public bool EnableSpatializer(bool enable, bool applyToTeam)
        {
            return _context.GetAudioCtrl().EnableSpatializer(enable, applyToTeam) == 0;
        }

        public bool InitSpatializer()
        {
            var modelPath = Application.persistentDataPath + "/GME_2.8_3d_model.dat";
            if (File.Exists(modelPath))
            {
                return _context.GetAudioCtrl().InitSpatializer(modelPath) == QAVError.OK;
            }
            var textAsset = Resources.Load<TextAsset>("GME_2.8_3d_model");
            if (textAsset == null)
            {
                Debug.Log("can't find resources dat file");
                return false;
            }
            var binaryWrite = new BinaryWriter(new FileStream(modelPath, FileMode.CreateNew));
            try
            {
                binaryWrite.Write(textAsset.bytes);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
            finally
            {
                binaryWrite.Close();
            }
            return _context.GetAudioCtrl().InitSpatializer(modelPath) == QAVError.OK;
        }
    }
}