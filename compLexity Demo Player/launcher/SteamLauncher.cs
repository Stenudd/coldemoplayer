﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace compLexity_Demo_Player
{
    public abstract class SteamLauncher : Launcher
    {
        protected override void VerifyProgramSettings()
        {
            // verify that steam.exe exists
            if (!File.Exists(Config.Settings.SteamExeFullPath))
            {
                throw new ApplicationException("Unable to find \"Steam.exe\" at \"" + Config.Settings.SteamExeFullPath + "\". Go to Options\\Preferences and select the correct Steam path.");
            }

            // verify that the steam account folder exists
            String steamAccountFullPath = Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\" + Config.Settings.SteamAccountFolder;

            if (!Directory.Exists(steamAccountFullPath))
            {
                throw new ApplicationException("Steam account folder \"" + gameFullPath + "\"doesn't exist. Go to Options\\Preferences and select a valid Steam account folder.");
            }

            // verify there is steam app info for this server/demo (if it's a steam demo)
            SteamGameInfo steamGameInfo;
            String gameFolderName;

            if (JoiningServer)
            {
                steamGameInfo = Steam.GetGameInfo(ServerSourceEngine, ServerGameFolderName);
                gameFolderName = ServerGameFolderName;
            }
            else
            {
                steamGameInfo = Demo.SteamGameInfo;
                gameFolderName = Demo.GameFolderName;
            }

            if (steamGameInfo == null && (JoiningServer || Demo.Engine != Demo.EngineEnum.HalfLife))
            {
                String s = String.Format("No Steam information can be found for the game \"{0}\".", gameFolderName);

                if (JoiningServer)
                {
                    s += " Cannot join the server.";
                }
                else
                {
                    s += " The demo cannot be played.";
                }

                throw new ApplicationException(s);
            }

            // verify that the game folder exists
            gameFullPath = steamAccountFullPath + "\\" + steamGameInfo.GameFolderExtended + "\\" + gameFolderName;

            if (!Directory.Exists(gameFullPath))
            {
                throw new ApplicationException("The game \"" + steamGameInfo.GameName + "\" doesn't seem to be installed.\n\nGame folder \"" + gameFullPath + "\" doesn't exist.\n\nCheck that the correct Steam account is selected in Options\\Preferences.");
            }

            // hlae: verify the exe path is set
            if (UseHlae)
            {
                if (!File.Exists(Config.Settings.HlaeExeFullPath))
                {
                    throw new ApplicationException("Unable to find the HLAE executable \"hlae.exe\" at \"" + Config.Settings.HlaeExeFullPath + "\". Go to Options\\Preferences and select the correct HLAE executable path.");
                }
            }
        }

        protected override void VerifyRunningPrograms()
        {
            // verify that steam is running
            if (Common.FindProcess("Steam", Config.Settings.SteamExeFullPath) == null)
            {
                throw new ApplicationException("Steam is not running. Launch Steam, log into your account and try again.");
            }

            // verify that HLAE is not running
            if (UseHlae)
            {
                if (Common.FindProcess("hlae", Config.Settings.HlaeExeFullPath) != null)
                {
                    throw new ApplicationException("Cannot play a demo with HLAE when it's already running. Exit HLAE and try again.");
                }
            }

            // verify that the game is not running
            SteamGameInfo steamGameInfo;

            if (JoiningServer)
            {
                steamGameInfo = Steam.GetGameInfo(ServerSourceEngine, ServerGameFolderName);
            }
            else
            {
                steamGameInfo = Demo.SteamGameInfo;
            }

            // even if HLAE is being used, still need to check that game isn't already running
            processExeFullPath = Path.GetDirectoryName(Config.Settings.SteamExeFullPath) + "\\SteamApps\\" + Config.Settings.SteamAccountFolder + "\\" + steamGameInfo.GameFolderExtended + "\\";

            if ((JoiningServer && ServerSourceEngine) || (!JoiningServer && Demo.Engine == Demo.EngineEnum.Source))
            {
                processExeFullPath += "hl2.exe";
            }
            else
            {
                processExeFullPath += "hl.exe";
            }

            if (Common.FindProcess(Path.GetFileNameWithoutExtension(processExeFullPath), processExeFullPath) != null)
            {
                throw new ApplicationException("Cannot play a demo while \"" + steamGameInfo.GameName + "\" is already running. Exit the game and try again.");
            }

            if (UseHlae)
            {
                processExeFullPath = Config.Settings.HlaeExeFullPath;
            }
        }

        protected override void LaunchProgram()
        {
            SteamGameInfo steamGameInfo;
            String mapName;

            if (JoiningServer)
            {
                steamGameInfo = Steam.GetGameInfo(ServerSourceEngine, ServerGameFolderName);
                mapName = ServerMapName;
            }
            else
            {
                steamGameInfo = Demo.SteamGameInfo;
                mapName = Demo.MapName;
            }

            // calculate launch parameters
            launchParameters = (UseHlae ? "" : "-applaunch " + steamGameInfo.AppId + " ");

            if ((JoiningServer && Config.Settings.ServerBrowserStartListenServer && !ServerSourceEngine) || (!JoiningServer && (Config.Settings.PlaybackStartListenServer || Demo.GameFolderName == "tfc") && Demo.Engine != Demo.EngineEnum.Source))
            {
                launchParameters += "-nomaster +maxplayers 10 +sv_lan 1 +map " + mapName;
            }

            launchParameters += " +exec " + Config.Settings.LaunchConfigFileName + " " + Config.Settings.SteamAdditionalLaunchParameters;

            // launch the program process
            if (UseHlae)
            {
                Hlae hlae = new Hlae();
                hlae.ReadConfig();
                hlae.WriteConfig(gameFullPath.Remove(gameFullPath.LastIndexOf('\\')) + "\\hl.exe", Demo.GameFolderName);

                ProcessStartInfo startInfo = new ProcessStartInfo(Config.Settings.HlaeExeFullPath);
                startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(Config.Settings.HlaeExeFullPath);
                startInfo.Arguments = "-ipcremote";

                Process.Start(startInfo);
            }
            else
            {
                Process.Start(Config.Settings.SteamExeFullPath, launchParameters);
            }
        }
    }
}