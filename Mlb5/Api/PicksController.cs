﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Mlb5.Models;
using Mlb5.Tasks;

namespace Mlb5.Api
{
    [RoutePrefix("api/picks")]
    public class PicksController : ApiController
    {
        [Route("{year}/{month}/{day}")]
        [HttpGet]
        public async Task<IHttpActionResult> Index(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);

            var games = new List<Game>();

            using (var db = new Mlb5Context())
            {
                await MlbApi.ImportGamesIfNeeded(db, date);
                games = db.Games.Where(x => x.Date == date).ToList();
            }


            return Ok(games);
        }

        [Route("{year}/{month}/{day}/xml")]
        [HttpGet]
        public async Task<IHttpActionResult> Xml(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);

            var mlbApi = new MlbApi();
            var response = await mlbApi.GetGameFiles(date);

            return Ok(response);
        }

        [Route("{year}/{month}/{day}/json")]
        [HttpGet]
        public async Task<IHttpActionResult> Json(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);

            var mlbApi = new MlbApi();
            var response = await mlbApi.Get(date);

            return Ok(response);
        }

        [Route("make")]
        [HttpPost]
        public async Task<IHttpActionResult> MakePick(int id, string teamcode)
        {
            var identity = User.Identity as ClaimsIdentity;

            var userId = Convert.ToInt32(identity.Claims.First(c => c.Type == "userId").Value);


            using (var db = new Mlb5Context())
            {
                var game = db.Games.Single(x => x.Id == id);

                var picks = db.Picks.Where(x => x.Game.Date == game.Date);

                if (picks.Count() < 5)
                {
                    var pick = new Pick()
                    {
                        Game = game,
                        TeamCode = teamcode,
                        UserId = userId
                    };
                    db.Picks.Add(pick);
                    await db.SaveChangesAsync();
                    return Ok(true);
                }
                return Ok(false);
            }
        }

        [Route("remove")]
        [HttpPost]
        public async Task<IHttpActionResult> RemovePick(int id)
        {
            using (var db = new Mlb5Context())
            {
                var pick = db.Picks.Single(x => x.Id == id);
                if (pick.Status == PickStatus.New)
                {
                    db.Picks.Remove(pick);
                    await db.SaveChangesAsync();
                    return Ok(true);
                }
                return Ok(false);
            }
        }
    }
}
