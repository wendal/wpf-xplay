using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfXplay.bean
{
    public class PlayObj
    {
        public String type;

        public String id;

        public String libName;

        public long start;

        [JsonProperty("params")]
        public Dictionary<string, object> _params;

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
}
