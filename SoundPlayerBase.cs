using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;

namespace CommonLib
{
    public abstract class SoundPlayerBase
    {
        private static List<SoundPlayerBase> _listPlayer = new List<SoundPlayerBase>();
        public string _soundLocation { get; set; }
        public abstract Stream _stream { get; }
        private SoundPlayer _soundPlayer;
        private bool _mute;
        private static bool _globalMute;

        public static bool GlobalMute
        {
            get { return _globalMute; }
            set
            {
                _globalMute = value;
                if (value)
                {
                    _listPlayer.ForEach(p => p.Stop());
                }
            }
        }

        public SoundPlayerBase()
        {
            _listPlayer.Add(this);
        }

        public bool Mute
        {
            get { return _mute; }
            set
            {
                _mute = value;
                if (value)
                    Stop();
            }
        }

        private SoundPlayer _player
        {
            get
            {
                if (_soundPlayer == null)
                    _soundPlayer = LoadMedia();
                return _soundPlayer;
            }
        }

        private SoundPlayer LoadMedia()
        {
            if (_stream != null)
                return new SoundPlayer(_stream);
            if (!string.IsNullOrWhiteSpace(_soundLocation))
                return new SoundPlayer(_soundLocation);
            return new SoundPlayer();
        }

        public void Play()
        {
            if (GlobalMute) return;
            if (Mute) return;
            _player.Play();
        }

        public void PlayLoop()
        {
            if (GlobalMute) return;
            if (Mute) return;
            _player.PlayLooping();
        }

        public void Stop()
        {
            _player.Stop();
        }
    }
}
