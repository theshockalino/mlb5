﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mlb5.Models
{
    public class GamePick
    {
        public GamePick()
        {
            AwayTeam = new GamePickTeam();
            HomeTeam = new GamePickTeam();
        }

        public int Id { get; set; }
        public string StringId { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan ElapsedTime
        {
            get;
            set;
        }
        public DateTime EndTime { get; set; }

        public GamePickTeam AwayTeam { get; set; }
        public GamePickTeam HomeTeam { get; set; }
        public string ElapsedTimeString { get; set; }
        public int ElapsedTimeHours { get; set; }
        public int ElapsedTimeMinutes { get; set; }
        public bool Picked { get; set; }
        public int PickId { get; set; }
        public GameStatus Status { get; set; }

        public GamePickTeam GetPickedTeam()
        {
            if (AwayTeam.Picked)
                return AwayTeam;
            else
            {
                return HomeTeam;
            }
        }

        public bool MarkPicked(Pick pick, Mlb5Context db)
        {
            Picked = true;
            PickId = pick.Id;
            if (AwayTeam.Code == pick.TeamCode)
            {
                AwayTeam.MarkPicked();
            }
            else
            {
                HomeTeam.MarkPicked();
            }

            if (Status == GameStatus.Completed && pick.Status == PickStatus.New)
            {
                var pickToUpdate = db.Picks.Single(x => x.Id == pick.Id);
                // if pick won then mark run and calculate runs
                if (AwayTeam.Runs > HomeTeam.Runs && AwayTeam.Picked)
                {
                    pickToUpdate.Status = PickStatus.Won;
                    pickToUpdate.Runs = AwayTeam.Runs - HomeTeam.Runs;
                }
                else if (HomeTeam.Runs > AwayTeam.Runs && HomeTeam.Picked)
                {
                    pickToUpdate.Status = PickStatus.Won;
                    pickToUpdate.Runs = HomeTeam.Runs - AwayTeam.Runs;
                }
                else
                {
                    pickToUpdate.Status = PickStatus.Lost;
                }
                // add homers and strikeouts
                var myPick = GetPickedTeam();
                pickToUpdate.Homeruns = myPick.Homeruns;
                pickToUpdate.Strikeouts = myPick.Strikeouts;

                db.SaveChanges();

                return true;
            }
            return false;
        }

        
        public void SetStatus(DateTime currentDateTime)
        {
            if (StartTime < currentDateTime)
            {
                if (EndTime < currentDateTime)
                {
                    Status = GameStatus.Completed;
                }
                else
                {
                    Status = GameStatus.Progress;
                }
            }
            
        }
    }

    public enum GameStatus
    {
        New,
        Progress,
        Completed
    }

    public class GamePickTeam
    {
        public int TeamId { get; set; }
        public string Code { get; set; }
        public string City { get; set; }
        public string Name { get; set; }
        public int Runs { get; set; }
        public int Homeruns { get; set; }
        public int Strikeouts { get; set; }
        public string Record { get; set; }
        public bool Picked { get; set; }


        public void MarkPicked()
        {
            Picked = true;
        }
    }

}