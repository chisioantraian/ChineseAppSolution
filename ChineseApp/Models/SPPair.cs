using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace ChineseApp.Models
{
    public class SPPair
    {
        public char ChineseCharacter { get; set; }
        public string Pinyin { get; set; }
        public Brush CharacterColor { get; set; }
        public string SimplifiedWord { get; set; }
    }
}
