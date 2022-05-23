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
                Session["ArtistFieldToSort"] = "names"; 
            if (Session["ArtistFieldToSortDir"] == null)
                Session["ArtistFieldToSortDir"] = true; // ascendant
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
            }
            return null;
        }

        public ActionResult ViewArtistLayout()
        {
            Session["matrixView"] = !(bool)Session["matrixView"];
            return null;
        }

        public ActionResult Index()
        {
            InitSortArtist();
            InitSearchByName();
            Session["matrixView"] = Session["matrixView"] == null ? false : true;
            return View();
        }
        public ActionResult GetArtistsList(bool forceRefresh = false)
        {
            List<Artiste> artistes = DB.Artistes.ToList();
            if (forceRefresh || !IsArtistUpToDate())
            {
                SetLocalArtistSerialNumber();
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
                
                return PartialView(artistes.Where(a => a.Approved).ToList());
            }
            return null;
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

                // visite


                HandleVisits(artist, id);

                
                SetLocalVideosSerialNumber();
                return PartialView(artist);
            }
            return null;
        }

        public void HandleVisits(Artiste artiste, int id)
        {
            var visites = (List<int>)HttpContext.Session["visites"];
            if (!visites.Contains(id) && IsFan(artiste) != -1)
            {
                visites.Add(id);
                artiste.Visits++;
                DB.SaveChanges();
            }
            HttpContext.Session["visites"] = visites;
        }

        public int IsFan(Artiste artist) => OnlineUsers.CurrentUserId == artist.UserId ? 
            -1 : DB.FanLikes.Where(a => a.UserId == OnlineUsers.CurrentUserId && a.ArtistId == artist.Id).FirstOrDefault() != null ? 1 : 0;

        public int IsAdmin() => DB.Users.Find(OnlineUsers.CurrentUserId).UserTypeId == 1 ? 1 : 0;

       
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
            artiste.Save();
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
                        DB.FanLikes.Remove(likes);
                        artiste.Likes--;
                        DB.SaveChanges();
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
        public ActionResult GroupEmail()
        {
            Artiste artiste = DB.Artistes.Where(a => OnlineUsers.CurrentUserId == a.UserId).FirstOrDefault();
            ViewBag.SelectedUsers = new List<int>();
            List<User> userList = new List<User>();
            foreach (FanLike fan in artiste.FanLikes)
            {
                userList.Add(fan.User);
            }
            ViewBag.Users = userList;
            return View(new GroupEmail());
        }
        [HttpPost]
        public ActionResult GroupEmail(GroupEmail groupEmail, List<int> SelectedUsers)
        {
            Artiste artiste = DB.Artistes.Where(a => OnlineUsers.CurrentUserId == a.UserId).FirstOrDefault();
            if (ModelState.IsValid)
            {
                groupEmail.SelectedUsers = SelectedUsers;
                groupEmail.Send(DB);
                return RedirectToAction("UserList");
            }
            List<User> userList = new List<User>();
            foreach (FanLike fan in artiste.FanLikes)
            {
                userList.Add(fan.User);
            }
            ViewBag.Users = userList;
            ViewBag.SelectedUsers = SelectedUsers;
   
            return View(groupEmail);
        }
        public ActionResult UserList()
        {
            return View();
        }

        [AdminAccess]
        public ActionResult ChangeUserBlockedStatus(int userid, bool blocked)
        {
            User user = DB.FindUser(userid);
            user.Blocked = blocked;
            DB.Update_User(user);
            return null;
        }

        public ActionResult Delete(int userId)
        {
            Artiste artiste = DB.Artistes.Where(a=> a.UserId == userId).FirstOrDefault();
            DB.Delete_Artiste(artiste);
            DB.RemoveUser(userId);
            return null;
        }

        [AdminAccess(false)] // RefreshTimout = false otherwise periodical refresh with lead to never timed out session
        public ActionResult GetUsersList(bool forceRefresh = false)
        {
            if (forceRefresh || OnlineUsers.NeedUpdate())
            {
                return PartialView(DB.Artistes.ToList());
            }
            return null;
        }

        [AdminAccess]
        public ActionResult ChangeUserAccess(int userid)
        {
            User user = DB.FindUser(userid);
            user.UserTypeId = (user.UserTypeId + 1) % 3 + 1;
            DB.Update_User(user);
            return null;
        }
        [AdminAccess]
        public ActionResult EditProfil(int id)
        {
            ViewBag.Genders = SelectListItemConverter<Gender>.Convert(DB.Genders.ToList());
            User user = DB.Users.Find(id);
            if (user != null)
            {
                UserClone uc = new UserClone(user);
                return View(uc);
            }
            else
                return null;
        }

        [HttpPost]
        public ActionResult EditProfil(UserClone uc)
        {
            if (ModelState.IsValid)
            {
                User user = DB.Users.Find(uc.Id);
                uc.CopyToUser(user);
                DB.Update_User(user);
                return RedirectToAction("UserList");
            }
            ViewBag.Genders = SelectListItemConverter<Gender>.Convert(DB.Genders.ToList());
            return View(uc);
        }

    }

}
