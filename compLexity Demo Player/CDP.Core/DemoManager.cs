﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using Ninject.Core;

namespace CDP.Core
{
    public class DemoManager
    {
        private class Plugin
        {
            public uint Priority { get; private set; }
            public Type DemoType { get; private set; }
            public DemoHandler DemoHandler { get; private set; }
            public Type LauncherType { get; private set; }

            public Plugin(uint priority, Type demoType, DemoHandler demoHandler, Type launcherType)
            {
                Priority = priority;
                DemoType = demoType;
                DemoHandler = demoHandler;
                LauncherType = launcherType;
            }
        }

        private readonly IFileSystem fileSystem;
        private readonly List<Plugin> plugins = new List<Plugin>();

        public DemoManager(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void AddPlugin(uint priority, Type demoType, DemoHandler demoHandler, Type launcherType)
        {
            if (!demoType.IsSubclassOf(typeof(Demo)))
            {
                throw new ArgumentException("Type must inherit from CDP.Core.Demo.", "demoType");
            }

            if (!launcherType.IsSubclassOf(typeof(Launcher)))
            {
                throw new ArgumentException("Type must inherit from CDP.Core.Launcher.", "launcherType");
            }

            plugins.Add(new Plugin(priority, demoType, demoHandler, launcherType));
        }

        public Setting[] GetAllDemoHandlerSettings()
        {
            List<Setting> settings = new List<Setting>();

            foreach (Plugin plugin in plugins)
            {
                settings.AddRange(plugin.DemoHandler.Settings);
            }

            return settings.Distinct().ToArray();
        }

        public string[] ValidDemoExtensions()
        {
            List<string> extensions = new List<string>();

            foreach (Plugin plugin in plugins)
            {
                foreach (string extension in plugin.DemoHandler.Extensions)
                {
                    extensions.Add(extension);
                }
            }

            return extensions.Distinct().ToArray();
        }

        public Demo CreateDemo(string fileName)
        {
            Plugin plugin = FindPlugin(fileName);

            if (plugin == null)
            {
                return null;
            }

            Demo demo = (Demo)Activator.CreateInstance(plugin.DemoType);
            demo.FileName = fileName;
            demo.Handler = plugin.DemoHandler;
            return demo;
        }

        public Launcher CreateLauncher(Demo demo)
        {
            Plugin plugin = plugins.SingleOrDefault(p => p.DemoType == demo.GetType());

            if (plugin == null)
            {
                throw new ApplicationException("Tried to create a launcher from an unknown demo type.");
            }

            try
            {
                return (Launcher)Activator.CreateInstance(plugin.LauncherType, demo);
            }
            catch (MissingMethodException)
            {
                throw new ApplicationException(string.Format("Launcher \"{0}\" is missing a constructor that takes a single parameter of type CDP.Core.Demo.", plugin.LauncherType));
            }
        }

        /// <summary>
        /// Find the highest priority plugin that can handle the given demo file.
        /// </summary>
        /// <param name="demoFileName">The demo filename.</param>
        /// <returns>A plugin, or null if no suitable plugin is found.</returns>
        private Plugin FindPlugin(string demoFileName)
        {
            string extension = fileSystem.GetExtension(demoFileName);

            using (Stream stream = fileSystem.OpenRead(demoFileName))
            {
                return plugins.Where(p =>
                {
                    if (!p.DemoHandler.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    return p.DemoHandler.IsValidDemo(stream);
                }).OrderByDescending(p => p.Priority).FirstOrDefault();
            }
        }
    }
}
