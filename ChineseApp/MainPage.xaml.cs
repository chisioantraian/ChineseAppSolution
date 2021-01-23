using ChineseApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



namespace ChineseApp
{

    public sealed partial class MainPage : Page
    {
        private string wordsPath = @".\Assets\allWords.utf8";
        private List<Word> allWords;

        public MainPage()
        {
            this.InitializeComponent();
            this.allWords = BuildAllWords();
            ApplicationView.GetForCurrentView().Title = $"{allWords.Count} words";
        }

        private List<Word> BuildAllWords()
        {
            return File.ReadAllLines(wordsPath)
                       .AsParallel()
                       .Select(WordFromLine)
                       .ToList();
        }
        private Word WordFromLine(string line)
        {
            string[] tokens = line.Split('\t');
            return new Word
            {
                Traditional = tokens[0],
                Simplified = tokens[1],
                Pinyin = tokens[2],
                Definitions = tokens[3],
                Frequency = int.Parse(tokens[4])
            };
        }

        private void SearchBar_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;

            if (string.IsNullOrEmpty(this.SearchBar.Text))
            {
                return;
            }

            ShowEnglishResult();
        }

        private void ShowEnglishResult()
        {
            string text = this.SearchBar.Text;
            var englishResult = GetEnglishResult(text);
            UpdateShownWords(englishResult);
        }

        private IEnumerable<Word> GetEnglishResult(string text)
        {
            return allWords.AsParallel()
                           .Where(w => ContainsInsensitive(w.Definitions, text));
        }

        private static bool ContainsInsensitive(string source, string value)
        {
            return source?.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void UpdateShownWords(IEnumerable<Word> filteredWords)
        {
            this.WordsList.ItemsSource = filteredWords.Select(ResultedWordFromWord_Main);
        }

        private ResultWord ResultedWordFromWord_Main(Word word)
        {
            return new ResultWord
            {
                SimplifiedPinyinPairs = GetSPPairsFromWord(word),
                DefinitionBlock = ComputeDefinitionBlock(word.Definitions)
            };
        }

        private static List<SPPair> GetSPPairsFromWord(Word word)
        {
            List<string> singlePron = word.Pinyin.Split(" ").ToList();
            List<SPPair> sPPairs = new List<SPPair>();

            for (int i = 0; i < word.Simplified.Length && i < singlePron.Count; i++)
            {
                sPPairs.Add(new SPPair
                {
                    ChineseCharacter = word.Simplified[i],
                    CharacterColor = ComputeColor(singlePron[i]),
                    Pinyin = singlePron[i],
                    SimplifiedWord = word.Simplified,
                    //CharPinyinMenu = CreateCharContextMenu(word.Simplified[i], word.Simplified)
                });
            }

            if (ShouldShowTraditional(word))
            {
                sPPairs.Add(new SPPair { ChineseCharacter = ' ', CharacterColor = new SolidColorBrush(Colors.Black), Pinyin = "", SimplifiedWord = word.Simplified });
                sPPairs.Add(new SPPair { ChineseCharacter = '〔', CharacterColor = new SolidColorBrush(Colors.DarkSlateGray), Pinyin = "", SimplifiedWord = word.Simplified });
                for (int i = 0; i < word.Traditional.Length && i < singlePron.Count; i++)
                {
                    if (word.Simplified[i] == word.Traditional[i])
                    {
                        sPPairs.Add(new SPPair
                        {
                            ChineseCharacter = '-',
                            CharacterColor = new SolidColorBrush(Colors.DarkSlateGray),//Brushes.DarkSlateGray,
                            Pinyin = "",
                            SimplifiedWord = word.Simplified
                        });
                    }
                    else
                    {
                        sPPairs.Add(new SPPair
                        {
                            ChineseCharacter = word.Traditional[i],
                            CharacterColor = ComputeColor(singlePron[i]),
                            Pinyin = singlePron[i],
                            SimplifiedWord = word.Simplified,
                            //CharPinyinMenu = CreateCharContextMenu(word.Traditional[i], word.Simplified)
                        });
                    }
                }
                sPPairs.Add(new SPPair { ChineseCharacter = '〕', CharacterColor = new SolidColorBrush(Colors.DarkSlateGray), Pinyin = "", SimplifiedWord = word.Simplified });
            }
            return sPPairs;
        }

        private static Brush ComputeColor(string pron)
        {
            if (pron.Contains("1")) return new SolidColorBrush(Colors.Red);
            if (pron.Contains("2")) return new SolidColorBrush(Colors.LimeGreen);
            if (pron.Contains("3")) return new SolidColorBrush(Colors.Blue);
            if (pron.Contains("4")) return new SolidColorBrush(Colors.DarkMagenta);
            return new SolidColorBrush(Colors.Gray);
        }

        private static bool ShouldShowTraditional(Word word)
        {
            for (int i = 0; i < word.Simplified.Length; i++)
            {
                if (word.Simplified[i] != word.Traditional[i])
                {
                    return true;
                }
            }
            return false;
        }

        private TextBlock ComputeDefinitionBlock(string definition)
        {
            TextBlock block = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) };

            string text = this.SearchBar.Text
                            .Replace("(", @"\(")
                            .Replace(")", @"\)");

            string pattern = $@"({text})";
            string[] substrings = Regex.Split(definition, pattern, RegexOptions.IgnoreCase);

            foreach (string match in substrings)
            {
                if (match.ToLower() == text.ToLower())
                {
                    block.Inlines.Add(new Run { Text = match, FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Colors.Orange) });
                }
                else
                {
                    block.Inlines.Add(new Run { Text = match, FontSize = 16, Foreground = new SolidColorBrush(Colors.Black), });
                }
            }
            return block;
        }
    }
}
