using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Linq.Mapping;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public class ProfileImage : INotifyPropertyChanged, IModelBuilderExtender
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        [Key]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual OsbideUser User { get; set; }

        private byte[] _profileImage;
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "image")]
        [DataMember]
        public byte[] Picture
        {
            get
            {
                return _profileImage;
            }
            set
            {
                _profileImage = value;
                OnPropertyChanged("ProfileImage");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void SetProfileImage(Bitmap bmp)
        {
            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            Picture = stream.ToArray();
        }

        public Bitmap GetProfileImage()
        {
            MemoryStream stream = new MemoryStream(Picture);
            Bitmap bmp = new Bitmap(stream);
            return bmp;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProfileImage>()
                .HasRequired(p => p.User)
                .WithRequiredDependent(u => u.ProfileImage)
                .WillCascadeOnDelete(true);
        }
    }
}
