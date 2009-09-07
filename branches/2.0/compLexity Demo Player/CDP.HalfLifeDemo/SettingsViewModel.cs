﻿using System;

namespace CDP.HalfLifeDemo
{
    public class SettingsViewModel : Core.ViewModelBase
    {
        protected readonly Core.ISettings settings;

        public bool Playdemo
        {
            get 
            {
                return ((Handler.PlaybackMethods)settings["HlPlaybackMethod"] == Handler.PlaybackMethods.Playdemo);
            }
            set
            {
                settings["HlPlaybackMethod"] = Handler.PlaybackMethods.Playdemo;
            }
        }

        public bool Viewdemo
        {
            get
            {
                return ((Handler.PlaybackMethods)settings["HlPlaybackMethod"] == Handler.PlaybackMethods.Viewdemo);
            }
            set
            {
                settings["HlPlaybackMethod"] = Handler.PlaybackMethods.Viewdemo;
            }
        }

        public bool StartListenServer
        {
            get { return (bool)settings["HlStartListenServer"]; }
            set { settings["HlStartListenServer"] = value; }
        }

        public bool RemoveShowscores
        {
            get { return (bool)settings["HlRemoveShowscores"]; }
            set { settings["HlRemoveShowscores"] = value; }
        }

        public SettingsViewModel(Core.ISettings settings)
        {
            this.settings = settings;
        }

        public override void Initialise()
        {
        }

        public override void Initialise(object parameter)
        {
        }
    }
}
