using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using WpfXplay.bean;

namespace WpfXplay
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, XCallback
    {

        XplayListener xs;

        public MainWindow()
        {
            InitializeComponent();
            xs = new XplayListener();
            xs.callback = this;
            Thread t = new Thread(xs.run);
            t.IsBackground = true;
            t.Start();
            Width = System.Windows.SystemParameters.PrimaryScreenWidth * PlayObj.scaling;
            Height = System.Windows.SystemParameters.PrimaryScreenHeight * PlayObj.scaling;

            Matrix matrix;
            var source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                matrix = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var src = new HwndSource(new HwndSourceParameters()))
                {
                    matrix = src.CompositionTarget.TransformToDevice;
                }
            }

            if (matrix != null)
            {
                PlayObj.dpiX = 96.0 * matrix.M11;
                PlayObj.dpiY = 96.0 * matrix.M22;
            }

            ContextMenu aMenu = new ContextMenu();
            MenuItem exitMenu = new MenuItem();
            exitMenu.Header = "退出";
            exitMenu.Click += (Object sender, RoutedEventArgs e) =>
            {
                Environment.Exit(0);
            };
            aMenu.Items.Add(exitMenu);
            TopCanvas.ContextMenu = aMenu;
        }

        public void HandlePlayObject(object obj)
        {
            PlayObj pobj = (PlayObj)obj;
            if (pobj._params == null)
                return;
            long start = pobj.start;
            if (start > 0)
            {
                long now = GetTimestamp(DateTime.UtcNow);
                int sleep = (int)(start - now);
                if (sleep > 5)
                {
                    Thread.Sleep(sleep - 5);
                }
            }

            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if ("stop" == pobj.type)
                {
                    StopLayout(pobj);
                }
                else if ("play" == pobj.type)
                {
                    StartLayout(pobj.getIntParam("zIndex"), pobj);
                }
                else
                {
                    Console.Out.WriteLine("what? type=" + pobj.type);
                }
            }));
            
        }

        public void StopLayout(PlayObj pobj)
        {
            object[] tmp = pobj._params["ids"] as object[];
            //Console.Out.WriteLine("tmp=" + tmp.GetType());
            foreach (string id in tmp)
            {
                int zIndex = int.Parse(id);

                UIElement ele = getByZIndex(zIndex);
                if (ele != null)
                {
                    TopCanvas.Children.Remove(ele);
                    TopCanvas.UnregisterName("LV_" + zIndex);
                }
            }
                    
        }

        public void StartLayout(int zIndex, PlayObj pobj)
        {
            string libName = pobj.libName;
            UIElement ele = getByZIndex(zIndex);
            bool isNew = ele == null;
            if (ele == null)
            {
                switch (libName)
                {
                    case "text":
                        ele = new TextUiEle();
                        break;
                    case "video":
                        ele = new VideoUiEle();
                        break;
                    case "pic":
                        ele = new PicUiEle();
                        break;
                }
            }
            if (ele == null)
                return;
            XplayUiEle xe = (XplayUiEle)ele;
            xe.prepare(pobj);
            
            if (libName == "text")
            {
                Canvas.SetLeft(ele, pobj.getIntParam("left") * PlayObj.scaling * 96 / PlayObj.dpiX);
                Canvas.SetTop(ele, pobj.getIntParam("top") * PlayObj.scaling * 96 / PlayObj.dpiY - 20);
                Console.Out.WriteLine(""+(pobj.getIntParam("top") * PlayObj.scaling * 96 / PlayObj.dpiY));
            }
            else
            {
                Canvas.SetLeft(ele, pobj.getIntParam("left") * PlayObj.scaling);
                Canvas.SetTop(ele, pobj.getIntParam("top") * PlayObj.scaling);
            }
            
            Canvas.SetZIndex(ele, 0 - zIndex);

            if (isNew)
            {
                TopCanvas.Children.Add(ele);
                TopCanvas.RegisterName("LV_" + zIndex, ele);
            }
            xe.Play();
        }

        public UIElement getByZIndex(int zIndex)
        {
            return (UIElement)TopCanvas.FindName("LV_" + zIndex);
        }

        public long GetTimestamp(DateTime dateTime)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (dateTime.Ticks - dt1970.Ticks) / 10000;
        }
    }

    public interface XCallback
    {
        void HandlePlayObject(object obj);
    }


    public interface XplayUiEle
    {
        void prepare(PlayObj pobj);
        void Play();
        void Stop();
    }
}
