using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WpfXplay.bean
{
    [DataContract]
    public class BoxProfile
    {
        //private BoxGeneral general { get; set; }

        private BoxStatusbar statusbar { get; set; }

        //private BoxOutput output;

        private BoxDownload download { get; set; }

        //private BoxUpload upload;
    }

    [DataContract]
    public class BoxUpload
    {
    }

    [DataContract]
    public class BoxDownload
    {
        private FRefer defaultScreen { get; set; }

        private FRefer calendar { get; set; }

        //private FRefer cmd;

        //private FRefer matrix;

        //private FRefer ledTicker;

        //private FRefer[] file;

        //private FRefer4[] down_list;

        //private Map<String, FRefer> plugin;

        //private FRefer4[] jarplugin;

        [DataMember(Order = 4, Name = "__all_files__")]
        private FRefer[] allFiles { get; set; }
    }

    [DataContract]
    public class FRefer
    {
    }

    [DataContract]
    public class BoxOutput
    {
    }

    [DataContract]
    public class BoxStatusbar
    {
        private string items { get; set; }

        private string hide { get; set; }

        private string display { get; set; }

        private string effect { get; set; }
    }

    [DataContract]
    public class BoxGeneral
    {
        private long update { get; set; }

        //private string name { get; set; }

        //private string timezone { get; set; }

        private long ping { get; set; }

        //private long revive { get; set; }

       // private long command { get; set; }

        //private int[] monitors { get; set; }
    }

    [DataContract]
    public class EasyCheckResp
    {
        [DataMember]
        public Dictionary<string, object> easyreg;

        [DataMember]
        public Dictionary<string, object> box_conf;
    }

    [DataContract]
    public class PingResp
    {
        [DataMember]
        public bool ok;
        [DataMember]
        public long rev;
        [DataMember]
        public string boxid;
        [DataMember]
        public long cmd;
    }
}
