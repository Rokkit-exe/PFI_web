using MySpace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySpace.Controllers
{
    public class ArtistesController : Controller
    {
        MySpaceDBEntities DB = new MySpaceDBEntities();
        // GET: Artistes
        public void RenewVideosSerialNumber()
        {
            HttpRuntime.Cache["VideosSerialNumber"] = Guid.NewGuid().ToString();
        }
        public string GetVideosSerialNumber()
        {
            if (HttpRuntime.Cache["VideosSerialNumber"] == null)
            {
                RenewVideosSerialNumber();
            }
            return (string)HttpRuntime.Cache["VideosSerialNumber"];
        }
        public void SetLocalVideosSerialNumber()
        {
            Session["VideosSerialNumber"] = GetVideosSerialNumber();
        }
        public bool IsVideosUpToDate()
        {
            return ((string)Session["VideosSerialNumber"] == (string)HttpRuntime.Cache["VideosSerialNumber"]);
        }

        public ActionResult Index()
        {
            return View(DB.Artists.Where(a => a.Approved).ToList());
        }

        // GET: Artistes/Page/5
        public ActionResult Page(int id)
        {
            Artist artist = DB.Artists.Find(id);
            ViewBag.isFan = isFan(artist);
            ViewBag.isAdmin = isAdmin();
            return View(artist);
        }

        public int isFan(Artist artist) => OnlineUsers.CurrentUserId == artist.UserId ? 
            -1 : DB.FanLikes.Where(a => a.UserId == OnlineUsers.CurrentUserId && a.ArtistId == artist.Id).FirstOrDefault() != null ? 1 : 0;

        public int isAdmin() => DB.Users.Find(OnlineUsers.CurrentUserId).UserTypeId == 1 ? 1 : 0;

        // GET: Artistes/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Artistes/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult AddVideo(int artistId , string title , string link)
        {
            string youtubeId = "";
            if (link.Contains("https://www.youtube.com/watch?v="))
            {
                youtubeId = link.Replace("https://www.youtube.com/watch?v=", "");
                if (youtubeId.IndexOf("&") > -1)
                {
                    youtubeId = youtubeId.Substring(0, youtubeId.IndexOf("&"));
                }
            }
            if(youtubeId != "")
            {
                Video video = new Video
                {
                    Creation = DateTime.Now,
                    ArtistId = artistId,
                    Title = title,
                    YoutubeLink = youtubeId

                };
                DB.Add_Video(video);
            }
            return null;
            
        }
    }
}
