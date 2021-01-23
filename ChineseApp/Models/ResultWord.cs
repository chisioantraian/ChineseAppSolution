using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ChineseApp.Models
{
    public class ResultWord
    {
        public IEnumerable<SPPair> SimplifiedPinyinPairs { get; set; }
        public TextBlock DefinitionBlock { get; set; }
    }
}
