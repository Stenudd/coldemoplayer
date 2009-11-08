﻿using System;
using System.IO;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;

namespace CDP.IdTech3
{
    public class Handler : Core.DemoHandler
    {
        private class RegisteredCommand
        {
            public CommandIds Id { get; private set; }
            public Type Type { get; private set; }

            public RegisteredCommand(Type type)
            {
                Type = type;
                Command instance = CreateInstance();
                Id = instance.Id;

                if (instance.IsSubCommand && instance.ContainsSubCommands)
                {
                    throw new ApplicationException("A command cannot contain subcommands and be a subcommand as well.");
                }

                if (instance.HasFooter && !instance.ContainsSubCommands)
                {
                    throw new ApplicationException("A command can only have a footer if it contains subcommands.");
                }
            }

            public Command CreateInstance()
            {
                return (Command)Activator.CreateInstance(Type);
            }
        }

        public override string FullName
        {
            get { return "id Tech 3"; }
        }

        public override string Name
        {
            get { return "idtech3"; }
        }

        public override string[] Extensions
        {
            get { return new string[] { "dm3", "dm_48", "dm_66", "dm_67", "dm_68", "dm_73" }; }
        }

        public override Core.DemoHandler.PlayerColumn[] PlayerColumns
        {
            get
            {
                return new PlayerColumn[]
                {
                    new PlayerColumn("Name", "Name"),
                    new PlayerColumn("Head Model", "HeadModel"),
                    new PlayerColumn("Model", "Model")
                };
            }
        }

        public override Core.Setting[] Settings
        {
            get { return new Core.Setting[] { }; }
        }

        private readonly List<RegisteredCommand> registeredCommands = new List<RegisteredCommand>();

        public Handler()
        {
            RegisterCommand<Commands.SvcBaseline>();
            RegisterCommand<Commands.SvcConfigString>();
            RegisterCommand<Commands.SvcGameState>();
            RegisterCommand<Commands.SvcServerCommand>();
            RegisterCommand<Commands.SvcSnapshot>();
        }

        public override UserControl SettingsView
        {
            get { return null; }
        }

        public override bool IsValidDemo(Core.FastFileStreamBase stream, string fileExtension)
        {
            if (stream.Length < 8)
            {
                return false;
            }

            stream.Seek(4, SeekOrigin.Begin); // Skip sequence number.
            int messageLength = stream.ReadInt();

            if (messageLength > Message.MAX_MSGLEN)
            {
                return false;
            }

            return true;
        }

        public override Core.Demo CreateDemo()
        {
            return new Demo();
        }

        public override Core.Launcher CreateLauncher()
        {
            throw new NotImplementedException();
        }

        public override UserControl CreateAnalysisView()
        {
            return new Analysis.View();
        }

        public override Core.ViewModelBase CreateAnalysisViewModel(Core.Demo demo)
        {
            return new Analysis.ViewModel((Demo)demo);
        }

        public Command CreateCommand(CommandIds id)
        {
            RegisteredCommand command = registeredCommands.FirstOrDefault(c => id == c.Id);

            if (command == null)
            {
                return null;
            }

            return command.CreateInstance();
        }

        protected void RegisterCommand<T>() where T : Command
        {
            RegisteredCommand command = registeredCommands.FirstOrDefault(c => c.Type == typeof(T));

            // Remove if the command is already registered.
            // This allows game-specific handlers to override generic base handlers.
            if (command != null)
            {
                registeredCommands.Remove(command);
            }

            command = new RegisteredCommand(typeof(T));

            // Make sure this ID isn't already registered by another command type.
            if (registeredCommands.FirstOrDefault(c => c.Id == command.Id) != null)
            {
                throw new ApplicationException("Duplicate command ID.");
            }

            registeredCommands.Add(command);
        }
    }
}
