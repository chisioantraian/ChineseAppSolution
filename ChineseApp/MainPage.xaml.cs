using ChineseApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public enum SelectedLanguage
    {
        English,
        Chinese
    }

    public enum ChineseSystem
    {
        Simplified,
        Traditional
    }

    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private string wordsPath = @".\Assets\allWords.utf8";
        private List<Word> allWords;
        //private IEnumerable<ResultWord> shownWords;
        private SelectedLanguage selectedLanguage = SelectedLanguage.English;

        private delegate void ShowLanguageResult(string text);
        private ShowLanguageResult showLanguageResult;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private List<ResultWord> shownWords = new List<ResultWord>();
        public List<ResultWord> ShownWords
        {
            get => shownWords;
            set
            {
                shownWords = value;
                RaiseProperty(nameof(ShownWords));
            }
        }


        public MainPage()
        {
            this.InitializeComponent();
            this.allWords = BuildAllWords();
            this.showLanguageResult = ShowEnglishResult;
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

        private  async void SearchBar_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;

            if (string.IsNullOrEmpty(this.SearchBar.Text))
            {
                return;
            }
            //showLanguageResult(this.SearchBar.Text);
            //await Task.Run(() => showLanguageResult(this.SearchBar.Text));
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                () => showLanguageResult(this.SearchBar.Text));
        }

        private void ShowEnglishResult(string text)
        {
            //string text = this.SearchBar.Text;
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
            //this.ShownWords = filteredWords.Select(ResultedWordFromWord_Main).ToList();
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

        private void ShowRandom_Click(object sender, RoutedEventArgs e)
        {
            var randomWords = GetRandomWords();
            UpdateShownWords(randomWords);
        }

        private IEnumerable<Word> GetRandomWords()
        {
            var random = new Random();
            var result = new List<Word>();
            for (int i = 0; i < 20; i++)
            {
                int index = random.Next(allWords.Count);
                result.Add(allWords[index]);
            }
            return result;
        }

        private void ChangeToEnglishInput_Click(object sender, RoutedEventArgs e)
        {
            this.SearchBar.Text = "";
            this.SearchBar.PlaceholderText = "Enter your english word, then press enter";
            this.selectedLanguage = SelectedLanguage.English;
            this.showLanguageResult = ShowEnglishResult;
        }

        private void ChangeToChineseInput_Click(object sender, RoutedEventArgs e)
        {
            this.SearchBar.Text = "";
            this.SearchBar.PlaceholderText = "Enter your characters/pinyin, then press enter";
            this.selectedLanguage = SelectedLanguage.Chinese;
            this.showLanguageResult = ShowChineseResult;
        }

        private void ShowChineseResult(string text)
        {
            //string text = this.SearchBar.Text;
            IEnumerable<Word> words;
            if (IsProbablyPinyin(text))
            {
                words = SearchByPinyin(text);
            }
            else
            {
                words = SearchBySimplified(text);
            }
            UpdateShownWords(words);
        }

        private const string pinyin = "   ,. 0123456789/abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private bool IsProbablyPinyin(string text)
        {
            return text.Any(c => pinyin.Contains(c));
        }


        private IEnumerable<Word> SearchByPinyin(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<Word>();
            }
            string[] prons = text.ToLower().Trim().Split(' ');
            return allWords.AsParallel()
                           .Where(CheckIfPinyinMatches);

            bool CheckIfPinyinMatches(Word word)
            {
                string[] wordProns = word.Pinyin.ToLower().Split(' ');
                if (prons.Length != wordProns.Length)
                    return false;
                for (int i = 0; i <= prons.Length - 1; i++)
                {
                    if (!wordProns[i].StartsWith(prons[i]))
                        return false;
                    if (wordProns[i] == prons[i])
                        continue;
                    if (wordProns[i].Length != (prons[i].Length + 1))
                        return false;
                }
                return true;
            }
        }


        private IEnumerable<Word> SearchBySimplified(string text)
        {
            ChineseSystem writingSystem = ChineseSystem.Simplified;
            if (writingSystem == ChineseSystem.Simplified)
            {
                return this.allWords.AsParallel().Where(w => w.Simplified.Contains(text));
            }
            else
            {
                return this.allWords.AsParallel().Where(w => w.Traditional.Contains(text));
            }
        }

        private void CharacterAndPinyin_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            char character = textBlock.Text[0];

            if (IsNotExtraCharacter(character))
            {
                textBlock.FontWeight = FontWeights.Bold;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
            }
        }

        private bool IsNotExtraCharacter(char character)
        {
            return character != '〔' &&
                    character != '〕' &&
                    character != '-' &&
                    character != ' ' &&
                    character != '·';
        }

        private void CharacterAndPinyin_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            textBlock.FontWeight = FontWeights.Normal;
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void CharacterAndPinyin_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            TextBlock block = (TextBlock)sender;
            ApplicationView.GetForCurrentView().Title = $"{block.Text}";
            //FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(block);
            //flyoutBase.ShowAt(block);

        }

    }
}
