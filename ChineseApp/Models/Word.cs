using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseApp.Models
{
    public class Word
    {
        public string Traditional { get; set; }
        public string Simplified { get; set; }
        public string Pinyin { get; set; }
        public string Definitions { get; set; }
        public int Frequency { get; set; }
    }
}
