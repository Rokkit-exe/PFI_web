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
            return View(DB.Artistes.Where(a => a.Approved).ToList());
        }

        // GET: Artistes/Page/5
        public ActionResult Page(int id)
        {
            Artiste artist = DB.Artistes.Find(id);
            if (artist != null)
            {
                SetLocalVideosSerialNumber();
                return View(artist);
            }


            return null;
        }
        public PartialViewResult GetPage(int id, bool forceRefresh = false)
        {
            if (forceRefresh || !IsVideosUpToDate())
            {
                Artiste artist = DB.Artistes.Find(id);
                ViewBag.currentUserId = OnlineUsers.CurrentUserId;
                ViewBag.isFan = isFan(artist);
                ViewBag.isAdmin = isAdmin();
                ViewBag.videos = DB.Videos.Where(v => v.ArtistId == id).ToList();
                ViewBag.messages = DB.Messages.Where(v => v.ArtistId == id).ToList();
                ViewBag.GUID = new ImageGUIDReference(@"/ImagesData/Photos/", @"No_image.png", true).GetURL(artist.MainPhotoGUID.ToString(), false);
                SetLocalVideosSerialNumber();
                return PartialView(artist);
            }
            return null;
        }

        public int isFan(Artiste artist) => OnlineUsers.CurrentUserId == artist.UserId ? 
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
                RenewVideosSerialNumber();
            }
            return null;
            
        }

        public ActionResult RemoveVideo(int videoId)
        {
            Video video = DB.Videos.Find(videoId);
            if (video != null)
            {
                DB.remove_video(video);
                RenewVideosSerialNumber();
            }
            return null;
        }
        public ActionResult Profil()
        {
            Artiste artiste = DB.Artistes.Find(OnlineUsers.CurrentUserId);
            return View(artiste);
        }

        [HttpPost]
        public ActionResult Profil(Artiste artiste)
        {
            if (ModelState.IsValid)
            { 
                artiste = DB.Update_Artiste(artiste);
            }
            
            return View(artiste);
        }
        public ActionResult AddMessage(int artistId, string message)
        {
            Artiste artiste = DB.Artistes.Find(artistId);
            User user = DB.Users.Find(OnlineUsers.CurrentUserId);
                if(artiste != null)
                {

                    Message newMessage = new Message
                    {
                        Artiste = artiste,
                        ArtistId = artiste.Id,
                        Creation = DateTime.Now,
                        Text = message,
                        User = user,
                        UserId = user.Id

                    };
                    DB.Add_Message(newMessage);
                    RenewVideosSerialNumber();
                }
            return null;
           
        }
        public ActionResult DeleteMessage(int messageId)
        {
            Message message = DB.Messages.Find(messageId);

            if(message != null)
            {
                DB.Delete_Message(message);
                RenewVideosSerialNumber();
            }
            return null;
        }
    }
}
