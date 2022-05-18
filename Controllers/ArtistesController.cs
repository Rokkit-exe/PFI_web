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
        public void RenewArtistSerialNumber()
        {
            HttpRuntime.Cache["ArtistSerialNumber"] = Guid.NewGuid().ToString();
        }
        public string GetArtistSerialNumber()
        {
            if (HttpRuntime.Cache["ArtistSerialNumber"] == null)
            {
                RenewArtistSerialNumber();
            }
            return (string)HttpRuntime.Cache["ArtistSerialNumber"];
        }
        public void SetLocalArtistSerialNumber()
        {
            Session["ArtistSerialNumber"] = GetArtistSerialNumber();
        }
        public bool IsArtistUpToDate()
        {
            return ((string)Session["ArtistSerialNumber"] == (string)HttpRuntime.Cache["ArtistSerialNumber"]);
        }
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
        public void InitSortArtist()
        {
            if (Session["ArtistFieldToSort"] == null)
                Session["ArtistFieldToSort"] = "dates"; // "users", "ratings"
            if (Session["ArtistFieldToSortDir"] == null)
                Session["ArtistFieldToSortDir"] = false; // ascendant
        }



        public void InitSearchByName()
        {
            if (Session["name"] == null)
                Session["name"] = "";
        }

        public ActionResult SetSearchArtistName(string name)
        {
            Session["name"] = name.Trim().ToLower();
            RenewArtistSerialNumber();
            return null;
        }

        public ActionResult SortArtistsBy(string fieldToSort)
        {
            RenewArtistSerialNumber();
            if ((string)Session["ArtistFieldToSort"] == fieldToSort)
            {
                Session["ArtistFieldToSortDir"] = !(bool)Session["ArtistFieldToSortDir"];
            }
            else
            {
                Session["ArtistFieldToSort"] = fieldToSort;
                Session["ArtistFieldToSortDir"] = true;
            }
            return null;
        }

        public ActionResult ViewArtistLayout(bool list)
        {
            if (list)
            {

            }
            return null;
        }

        public ActionResult Index()
        {
            InitSortArtist();
            InitSearchByName();
            SetLocalVideosSerialNumber();
            return View();
        }
        public ActionResult GetArtistsList(bool forceRefresh = false)
        {
            List<Artiste> artistes = DB.Artistes.ToList();
            if (forceRefresh || !IsArtistUpToDate())
            {
                
                string name = (string)Session["name"];
                if (name != "" || name != null)
                    artistes = DBDAL.SearchArtistByName(artistes, name);
                switch ((string)Session["ArtistFieldToSort"])
                {
                    case "names":
                        if ((bool)Session["ArtistFieldToSortDir"])
                        {
                            artistes = artistes.OrderBy(pr => pr.Name).ToList();
                        }
                        else
                        {
                            artistes = artistes.OrderByDescending(pr => pr.Name).ToList();
                        }
                        break;
                    case "vues":
                        if ((bool)Session["ArtistFieldToSortDir"])
                        {
                            artistes = artistes.OrderBy(pr => pr.Visits).ThenBy(pr => pr.User.LastName).ToList();
                        }
                        else
                        {
                            artistes = artistes.OrderByDescending(pr => pr.Visits).ThenByDescending(pr => pr.User.LastName).ToList();
                        }
                        break;
                    case "likes":
                        if ((bool)Session["ArtistFieldToSortDir"])
                        {
                            artistes = artistes.OrderBy(pr => pr.Likes).ToList();
                        }
                        else
                        {
                            artistes = artistes.OrderByDescending(pr => pr.Likes).ToList();
                        }
                        break;
                    default:
                        break;

                        
                }
                SetLocalArtistSerialNumber();
                return PartialView(artistes.Where(a => a.Approved).ToList());
            }
            return PartialView(artistes.Where(a => a.Approved).ToList());
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
                ViewBag.isFan = IsFan(artist);
                ViewBag.isAdmin = IsAdmin();
                ViewBag.videos = DB.Videos.Where(v => v.ArtistId == id).ToList();
                ViewBag.messages = DB.Messages.Where(v => v.ArtistId == id).ToList();
                ViewBag.GUID = new ImageGUIDReference(@"/ImagesData/Photos/", @"No_image.png", true).GetURL(artist.MainPhotoGUID.ToString(), false);
                SetLocalVideosSerialNumber();
                return PartialView(artist);
            }
            return null;
        }

        public int IsFan(Artiste artist) => OnlineUsers.CurrentUserId == artist.UserId ? 
            -1 : DB.FanLikes.Where(a => a.UserId == OnlineUsers.CurrentUserId && a.ArtistId == artist.Id).FirstOrDefault() != null ? 1 : 0;

        public int IsAdmin() => DB.Users.Find(OnlineUsers.CurrentUserId).UserTypeId == 1 ? 1 : 0;

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
            Artiste artiste = DB.Artistes.Where(A => A.UserId == OnlineUsers.CurrentUserId).FirstOrDefault();
            return View(artiste);
        }

        [HttpPost]
        public ActionResult Profil(Artiste artiste)
        {
            artiste.User = DB.Users.Find(OnlineUsers.CurrentUserId);
            //artiste.MainPhotoGUID = "/ImagesData/Avatars/no_avatar.png";
            //if (ModelState.IsValid)
            //{ 
                artiste = DB.Update_Artiste(artiste);
            //}
            
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
        public ActionResult AddRemoveLike(int artistId)
        {
            Artiste artiste = DB.Artistes.Find(artistId);
            int currentUserId = OnlineUsers.CurrentUserId;
            if(artiste != null)
            {
                foreach(FanLike likes in artiste.FanLikes)
                {
                    if(likes.UserId == currentUserId)
                    {

                        artiste.FanLikes.Remove(likes);
                        artiste.Likes--;
                        return null;
                    }
                    
                }
                FanLike newLike = new FanLike
                {
                    Artiste = artiste,
                    ArtistId = artiste.Id,
                    Creation = DateTime.Now,
                    User = DB.Users.Find(currentUserId),
                    UserId = currentUserId
                };
                DB.Add_Like(newLike);
                RenewVideosSerialNumber();
                
            }
            return null;
        }


    }
}
