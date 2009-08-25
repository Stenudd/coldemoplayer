﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CDP.HalfLifeDemo
{
    public class Handler : Core.DemoHandler
    {
        public override string FullName
        {
            get { return "Half-Life"; }
        }

        public override string Name
        {
            get { return "halflife"; }
        }

        public override string[] Extensions
        {
            get { return new string[] { "dem" }; }
        }

        private readonly byte[] magic = { 0x48, 0x4C, 0x44, 0x45, 0x4D, 0x4F }; // HLDEMO
        private Dictionary<byte, Type> frames = new Dictionary<byte, Type>();
        private Dictionary<byte, Type> engineMessages = new Dictionary<byte, Type>();
        private Dictionary<string, Type> userMessages = new Dictionary<string, Type>();

        public Handler()
        {
            // Register frames.
            RegisterFrame(typeof(Frames.Loading));
            RegisterFrame(typeof(Frames.Playback));
            RegisterFrame(typeof(Frames.PlaybackSegmentStart));
            RegisterFrame(typeof(Frames.ClientCommand));
            RegisterFrame(typeof(Frames.ClientData));
            RegisterFrame(typeof(Frames.EndOfSegment));
            RegisterFrame(typeof(Frames.Unknown));
            RegisterFrame(typeof(Frames.WeaponChange));
            RegisterFrame(typeof(Frames.PlaySound));
            RegisterFrame(typeof(Frames.ModData));

            // Register engine messages.
            RegisterEngineMessage(typeof(Messages.SvcNop));
            RegisterEngineMessage(typeof(Messages.SvcSetView));
            RegisterEngineMessage(typeof(Messages.SvcSound));
            RegisterEngineMessage(typeof(Messages.SvcTime));
            RegisterEngineMessage(typeof(Messages.SvcPrint));
            RegisterEngineMessage(typeof(Messages.SvcStuffText));
            RegisterEngineMessage(typeof(Messages.SvcSetAngle));
            RegisterEngineMessage(typeof(Messages.SvcServerInfo));
            RegisterEngineMessage(typeof(Messages.SvcLightStyle));
            RegisterEngineMessage(typeof(Messages.SvcUpdateUserInfo));
            RegisterEngineMessage(typeof(Messages.SvcDeltaDescription));
            RegisterEngineMessage(typeof(Messages.SvcClientData));
            RegisterEngineMessage(typeof(Messages.SvcPings));
            RegisterEngineMessage(typeof(Messages.SvcSpawnBaseline));
            RegisterEngineMessage(typeof(Messages.SvcTempEntity));
            RegisterEngineMessage(typeof(Messages.SvcSignOnNum));
            RegisterEngineMessage(typeof(Messages.SvcSpawnStaticSound));
            RegisterEngineMessage(typeof(Messages.SvcCdTrack));
            RegisterEngineMessage(typeof(Messages.SvcNewUserMessage));
            RegisterEngineMessage(typeof(Messages.SvcPacketEntities));
            RegisterEngineMessage(typeof(Messages.SvcChoke));
            RegisterEngineMessage(typeof(Messages.SvcResourceList));
            RegisterEngineMessage(typeof(Messages.SvcNewMoveVars));
            RegisterEngineMessage(typeof(Messages.SvcResourceRequest));
            RegisterEngineMessage(typeof(Messages.SvcHltv));
            RegisterEngineMessage(typeof(Messages.SvcDirector));
            RegisterEngineMessage(typeof(Messages.SvcVoiceInit));
            RegisterEngineMessage(typeof(Messages.SvcSendExtraInfo));
            RegisterEngineMessage(typeof(Messages.SvcTimeScale));

            // Register user messages.
        }

        public override bool IsValidDemo(Stream stream)
        {
            for (int i = 0; i < magic.Length; i++)
            {
                if (stream.ReadByte() != magic[i])
                {
                    return false;
                }
            }

            return true;
        }

        public Frame CreateFrame(byte id)
        {
            if (!frames.ContainsKey(id))
            {
                return null;
            }

            return (Frame)Activator.CreateInstance(frames[id]);
        }

        public EngineMessage CreateEngineMessage(byte id)
        {
            if (id > Demo.MaxEngineMessageId)
            {
                throw new ApplicationException("Tried to create an engine message with an ID higher than MaxEngineMessageId.");
            }

            if (!engineMessages.ContainsKey(id))
            {
                return null;
            }

            return (EngineMessage)Activator.CreateInstance(engineMessages[id]);
        }

        public UserMessage CreateUserMessage(string name)
        {
            if (!userMessages.ContainsKey(name))
            {
                return new Messages.UnregisteredUserMessage(name);
            }

            return (UserMessage)Activator.CreateInstance(userMessages[name]);
        }

        private void RegisterFrame(Type type)
        {
            Frame instance = Activator.CreateInstance(type) as Frame;

            if (instance == null)
            {
                throw new ApplicationException("Specified type does not inherit from Frame.");
            }

            frames.Add(instance.Id, type);
        }

        private void RegisterEngineMessage(Type type)
        {
            EngineMessage instance = Activator.CreateInstance(type) as EngineMessage;

            if (instance == null)
            {
                throw new ApplicationException("Specified type does not inherit from Message.");
            }

            engineMessages.Add(instance.Id, type);
        }

        private void RegisterUserMessage(Type type)
        {

        }
    }
}
