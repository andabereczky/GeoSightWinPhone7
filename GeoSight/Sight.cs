using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GeoSight
{
    /// <summary>
    /// A class representing a sight.
    /// </summary>
    public class Sight : INotifyPropertyChanged
    {
        #region Member variables

        /// <summary>
        /// The ID of the sight.
        /// </summary>
        private int id;

        /// <summary>
        /// The name of the sight.
        /// </summary>
        private string name;

        /// <summary>
        /// The radius of the sight.
        /// </summary>
        private double radius;

        //private int userID;
        //private DateTime createdAt;
        //private DateTime updatedAt;

        /// <summary>
        /// The latitude of the sight.
        /// </summary>
        private double latitude;

        /// <summary>
        /// The longitude of the sight.
        /// </summary>
        private double longitude;

        /// <summary>
        /// The URL of the thumbnail image for the sight.
        /// </summary>
        private string thumbnailURL;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Accessors

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                if (value != id)
                {
                    id = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (value != radius)
                {
                    radius = value;
                    NotifyPropertyChanged("Radius");
                }
            }
        }

        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                if (value != latitude)
                {
                    latitude = value;
                    NotifyPropertyChanged("Latitude");
                }
            }
        }

        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                if (value != longitude)
                {
                    longitude = value;
                    NotifyPropertyChanged("Longitude");
                }
            }
        }

        public string ThumbnailURL
        {
            get
            {
                return thumbnailURL;
            }
            set
            {
                if (value != thumbnailURL)
                {
                    thumbnailURL = value;
                    NotifyPropertyChanged("ThumbnailURL");
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonSight">Information about a sight in JSON format.</param>
        public Sight(JToken jsonSight)
        {
            ID = Convert.ToInt32(jsonSight["id"].ToString());
            Name = jsonSight["name"].ToString();
            Radius = Convert.ToDouble(jsonSight["radius"].ToString());
            //yadda = jsonSight["created_at"];
            //yadda = jsonSight["updated_at"];
            //yadda = jsonSight["user_id"];
            Latitude = Convert.ToDouble(jsonSight["latitude"].ToString());
            Longitude = Convert.ToDouble(jsonSight["longitude"].ToString());
            ThumbnailURL = jsonSight["thumbnail"].ToString();
        }

        #endregion

        #region Private member functions

        /// <summary>
        /// Raise the PropertyChanged event and pass along the property that changed.
        /// </summary>
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}
