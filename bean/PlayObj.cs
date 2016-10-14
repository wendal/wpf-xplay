using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfXplay.bean
{
    [DataContract]
    public class PlayObj
    {
        [DataMember(Order = 0, IsRequired = true)]
        public String type;

        [DataMember(Order = 1)]
        public String id;

        [DataMember(Order = 2)]
        public String libName;

        [DataMember(Order = 3)]
        public long start;

        [DataMember(Order = 4, Name = "params")]
        public Dictionary<string, object> _params;

        [DataMember(Order = 5)]
        public List<Dictionary<string, object>> deps;

        public string getPath()
        {
            if (_params.ContainsKey("path"))
                return Path.GetFullPath(_params["path"].ToString());
            else if (deps.Count > 0)
            {
                return deps[0]["path"].ToString();
            }
            return "https://nutz.cn";
        }

        public int getIntParam(string key)
        {
            return int.Parse(_params[key].ToString());
        }

        public double getDisplayParamReal(string key)
        {
            return int.Parse(_params[key].ToString()) * PlayObj.scaling * 96 / PlayObj.dpiY;
        }

        public string getStringParam(string key)
        {
            return _params[key].ToString();
        }

        public static double scaling = 0.5;
        public static double dpiX = 96, dpiY = 96;
        
    }

    public class VideoUiEle : MediaElement, XplayUiEle
    {
        public void prepare(PlayObj pobj)
        {
            this.LoadedBehavior = MediaState.Manual;
            this.Source = new Uri(pobj.getPath());
            Width = pobj.getDisplayParamReal("width");
            Height = pobj.getDisplayParamReal("height");
        }
        
    }


    public class PicUiEle : Image, XplayUiEle
    {

        public void prepare(PlayObj pobj)
        {
            Stretch = Stretch.Fill;
            StretchDirection = StretchDirection.Both;
            Height = pobj.getDisplayParamReal("height");
            Width = pobj.getDisplayParamReal("width");
            Source = new BitmapImage(new Uri(pobj.getPath()));
        }

        public void Play() {
            
        }
        public void Stop() { }
    }

    public class TextUiEle : TextBlock, XplayUiEle
    {
        public void prepare(PlayObj pobj)
        {
            Height = pobj.getDisplayParamReal("height");
            Width = pobj.getDisplayParamReal("width");
            FontSize = pobj.getDisplayParamReal("font_size");

            Text = pobj.getStringParam("content");

            TextAlignment = System.Windows.TextAlignment.Center;
        }

        public void Play()
        {

        }
        public void Stop() { }
    }

    public static class Json
    {

        public static T fromJson<T>(string jsonString)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return (T)new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                }).ReadObject(ms);
            }
        }

        public static string toJson(object jsonObject)
        {
            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(jsonObject.GetType()).WriteObject(ms, jsonObject);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
