using MySpace.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySpace.Models
{
    [MetadataType(typeof(ArtisteView))]
    public partial class Artiste
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Artiste()
        {
            this.FanLikes = new HashSet<FanLike>();
            this.Messages = new HashSet<Message>();
            this.Videos = new HashSet<Video>();
        }

        [NotMapped]
        public string Data { get; set; }
        private static ImageGUIDReference PhotoReference = new ImageGUIDReference(@"/ImagesData/Photos/", @"no_image.png", true);

        public string GetUrl(bool thumbnail = false)
        {
            return PhotoReference.GetURL(MainPhotoGUID, thumbnail);
        }

        public void Save()
        {
            MainPhotoGUID = PhotoReference.SaveImage(Data, MainPhotoGUID);
        }

        public void Remove()
        {
            PhotoReference.Remove(MainPhotoGUID);
        }
    }


    public class ArtisteView
    {
        [Display(Name="Id") , Required(ErrorMessage = "Obligatoire")]
        public int Id { get; set; }
        [Display(Name = "UserId"), Required(ErrorMessage = "Obligatoire")]
        public int UserId { get; set; }
        [Display(Name = "Name"), Required(ErrorMessage = "Obligatoire")]
        public string Name { get; set; }
        [Display(Name = "MainPhotoGUID")]
        public string MainPhotoGUID { get; set; } = "/ImagesData/Avatars/no_avatar.png";
        [Display(Name = "Data")]
        [NotMapped]
        public string Data { get; set; }
        [Display(Name = "Description"), Required(ErrorMessage = "Obligatoire")]
        public string Description { get; set; }
        [Display(Name = "Approved"), Required(ErrorMessage = "Obligatoire")]
        public bool Approved { get; set; } = false;
        [Display(Name = "Visits")]
        public Nullable<int> Visits { get; set; } = 0;
        [Display(Name = "Likes")]
        public Nullable<int> Likes { get; set; } = 0;

        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FanLike> FanLikes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Message> Messages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Video> Videos { get; set; }

    }
}