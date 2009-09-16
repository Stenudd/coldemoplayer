﻿using System;
using System.Linq;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;

namespace CDP.HalfLifeDemo.Analysis
{
    public class ViewModel : Core.ViewModelBase
    {
        public class Player
        {
            public byte Slot { get; set; }
            public int EntityId { get; set; }
            public string Name { get; set; }
        }

        public FlowDocument GameLogDocument { get; private set; }
        public List<Scoreboard.Round> Rounds { get; private set; }
        public List<Player> Players { get; private set; }

        private readonly Demo demo;
        private float currentTimestamp = 0.0f; // TODO: need demo event handler for when gamedata frame is read to get this.

        protected readonly Core.IFlowDocumentWriter gameLog = Core.ObjectCreator.Get<Core.IFlowDocumentWriter>();

        public ViewModel(Demo demo)
        {
            this.demo = demo;
            this.demo.AddMessageCallback<Messages.SvcPrint>(MessagePrint);
            this.demo.AddMessageCallback<Messages.SvcUpdateUserInfo>(MessageUpdateUserInfo);
            this.demo.AddMessageCallback<UserMessages.ScoreInfo>(MessageScoreInfo);
            this.demo.AddMessageCallback<UserMessages.TeamInfo>(MessageTeamInfo);
            this.demo.AddMessageCallback<UserMessages.TeamScore>(MessageTeamScore);
            GameLogDocument = new FlowDocument();
            Rounds = new List<Scoreboard.Round>();
            Players = new List<Player>();
            NewRound();
        }

        public override void OnNavigateComplete()
        {
            gameLog.Save(GameLogDocument);
            OnPropertyChanged("GameLogDocument");
        }

        protected void NewRound()
        {
            Scoreboard.Round lastRound = Rounds.LastOrDefault();
            Scoreboard.Round newRound = new Scoreboard.Round(Rounds.Count + 1, currentTimestamp);
            Rounds.Add(newRound);

            // Carry over all player and team scores from the previous round (except disconnected players).
            if (lastRound != null)
            {
                foreach (Scoreboard.Team team in lastRound.Teams)
                {
                    newRound.Teams.Add((Scoreboard.Team)team.Clone());
                }

                foreach (Scoreboard.Player player in lastRound.Players)
                {
                    if (player.IsConnected)
                    {
                        newRound.Players.Add((Scoreboard.Player)player.Clone());
                    }
                }
            }

            if (newRound.Number != 1)
            {
                gameLog.Write("\n");
            }

            GameLogWriteTimestamp();
            gameLog.Write(string.Format("Round {0}:\n", newRound.Number), Brushes.Black, TextDecorations.Underline);
        }

        protected void GameLogWriteTimestamp()
        {
            gameLog.Write(string.Format("{0}: ", currentTimestamp), Brushes.Brown);
        }

        #region Message handlers
        private void MessagePrint(Messages.SvcPrint message)
        {
            gameLog.Write(message.Text + "\n");
        }

        private void MessageUpdateUserInfo(Messages.SvcUpdateUserInfo message)
        {
            // FIXME: handle infokey parsing elsewhere.
            string[] infoKeys = message.Info.Split('\\');
            string name = null;

            for (int i = 0; i < infoKeys.Length; i++)
            {
                if (infoKeys[i] == "name")
                {
                    name = infoKeys[i + 1];
                    break;
                }
            }

            byte slot = (byte)(message.Slot + 1);

            // Global player state.
            Player player = Players.FirstOrDefault(p => p.EntityId == message.EntityId);

            if (player == null)
            {
                player = new Player
                {
                    Slot = slot,
                    EntityId = message.EntityId,
                    Name = name
                };

                Players.Add(player);
            }

            // Scoreboard.
            Scoreboard.Player sbPlayer = Rounds.Last().Players.FirstOrDefault(p => p.Slot == slot);

            if (sbPlayer == null)
            {
                sbPlayer = new Scoreboard.Player(slot);
                Rounds.Last().Players.Add(sbPlayer);
            }
            else if (string.IsNullOrEmpty(message.Info))
            {
                sbPlayer.IsConnected = false;
                return;
            }

            sbPlayer.Name = name;
            sbPlayer.IsConnected = true; // Possible mid-round reconnect.
        }

        private void MessageScoreInfo(UserMessages.ScoreInfo message)
        {
            Scoreboard.Player player = Rounds.Last().Players.FirstOrDefault(p => p.Slot == message.Slot);

            if (player != null && player.IsConnected)
            {
                player.Frags = message.Frags;
                player.Deaths = message.Deaths;
            }
        }

        private void MessageTeamInfo(UserMessages.TeamInfo message)
        {
            Scoreboard.Player player = Rounds.Last().Players.FirstOrDefault(p => p.Slot == message.Slot);

            if (player != null)
            {
                player.TeamName = message.TeamName;
            }
        }

        private void MessageTeamScore(UserMessages.TeamScore message)
        {
            Scoreboard.Team team = Rounds.Last().Teams.FirstOrDefault(t => t.Name == message.TeamName);

            if (team == null)
            {
                team = new HalfLifeDemo.Analysis.Scoreboard.Team(message.TeamName, message.Score);
                Rounds.Last().Teams.Add(team);
            }
            else
            {
                team.Score = message.Score;
            }
        }
        #endregion
    }
}
