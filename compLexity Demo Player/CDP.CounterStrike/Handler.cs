﻿using System;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CDP.CounterStrike
{
    public class Handler : HalfLife.Handler
    {
        public override string FullName
        {
            get { return "Counter-Strike"; }
        }

        public override string Name
        {
            get { return "cstrike"; }
        }

        public override Core.Setting[] Settings
        {
            get
            {
                return new Core.Setting[]
                {
                    new Core.Setting("CsRemoveFadeToBlack", typeof(bool), true)
                };
            }
        }

        private UserControl settingsView;
        public override UserControl SettingsView
        {
            get
            {
                if (settingsView == null)
                {
                    settingsView = new SettingsView { DataContext = new SettingsViewModel() };
                }

                return settingsView;
            }
        }

        private Game game;

        public Handler()
        {
        }

        public override bool IsValidDemo(Core.FastFileStreamBase stream, string fileExtension)
        {
            if (!base.IsValidDemo(stream, fileExtension))
            {
                return false;
            }

            // magic (8 bytes) + demo protocol (4 bytes) + network protocol (4 bytes) + map name (260 bytes) = 276
            // game folder = 260
            if (stream.BytesLeft < 276 + 260)
            {
                return false;
            }

            stream.Seek(276, SeekOrigin.Begin);
            return game.DemoGameFolders.Contains(stream.ReadString(), StringComparer.InvariantCultureIgnoreCase);
        }

        public override Core.Demo CreateDemo()
        {
            return new Demo();
        }

        public override Core.Launcher CreateLauncher()
        {
            return new Launcher();
        }

        public override Core.ViewModelBase CreateAnalysisViewModel(Core.Demo demo)
        {
            return new Analysis.ViewModel((Demo)demo);
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            RegisterEngineMessage<Messages.SvcClientData>();
            RegisterUserMessage<UserMessages.ClCorpse>();
            RegisterUserMessage<UserMessages.SendAudio>();
        }

        protected override void ReadGameConfig()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Game));

            using (StreamReader stream = new StreamReader(fileSystem.PathCombine(settings.ProgramPath, "config", "goldsrc", "cstrike.xml")))
            {
                game = (Game)serializer.Deserialize(stream);
            }
        }

        public override Core.SteamGame FindGame(string gameFolder)
        {
            return game;
        }
    }
}
