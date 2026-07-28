using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace languageFlashCards
{
    public partial class Form1 : Form
    {

        private class WordPair
        {
            public int RowIndex { get; set; }
            public string foreign { get; set; }
            public string pronunciation { get; set; }
            public string english { get; set; }
            public int CorrectStreak { get; set; }
        }
        public class LanguageFileFormat
        {
            public string FilePath { get; init; }
            public string Delimiter { get; init; }
            public int ForeignIndex { get; init; }
            public int? PronunciationIndex { get; init; }
            public int EnglishIndex { get; init; }
            public int StreakIndex { get; init; }
            public int TextSize { get; init; }
            public bool ShowPronunciationByDefault { get; init; }
        }

        public static readonly List<LanguageFileFormat> languages = new()
        {
            new LanguageFileFormat
            {
                // actual file in workspace: data\japanese working.tsv
                FilePath =  @"C:\code\languageFlashCards\data\jp 2.tsv",
                Delimiter = "\t",

                ForeignIndex = 0,
                PronunciationIndex = 2,
                EnglishIndex = 1,
                StreakIndex = 3,
                TextSize = 175,
                ShowPronunciationByDefault = false
            },
            new LanguageFileFormat
            {
                // actual file in workspace: data\japanese working.tsv
                FilePath =  @"C:\code\languageFlashCards\data\jp sentences.tsv",
                Delimiter = "\t",

                ForeignIndex = 1,
                PronunciationIndex = 0,
                EnglishIndex = 2,
                StreakIndex = 3,
                TextSize = 105,
                ShowPronunciationByDefault = true
            },/*
            new LanguageFileFormat
            {
                // actual file in workspace: data\japanese working.tsv
                FilePath =  @"C:\code\languageFlashCards\data\japanese working.tsv",
                Delimiter = "\t",

                ForeignIndex = 3,
                PronunciationIndex = 4,
                EnglishIndex = 5,
                StreakIndex = 12,
                TextSize = 150
            },*//*
            new LanguageFileFormat
            {
                FilePath =  @"C:\code\languageFlashCards\data\japanese temp.psv",
                Delimiter = "|",

                ForeignIndex = 0,
                PronunciationIndex = 1,
                EnglishIndex = 2,
                StreakIndex = 3,
                TextSize = 100
            },*/
            new LanguageFileFormat
            {
                // actual file in workspace: data\french_words.csv
                FilePath =  @"C:\code\languageFlashCards\data\french_words.csv",
                Delimiter = ",",

                ForeignIndex = 0,
                PronunciationIndex = null,
                EnglishIndex = 1,
                StreakIndex = 2,
                TextSize = 60,
                ShowPronunciationByDefault = false
            }
        };


        private List<WordPair> words;
        private int selectedLanguageIndex = 0;
        private LanguageFileFormat currentLanguage;

        private string[] allLines;
        private Random rand = new Random();
        private WordPair currentWord;
        private WordPair previousWord;
        private bool isClicked = false;
        private string completeAnswer = string.Empty;

        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true; // important to let the form capture key presses
            this.KeyDown += Form1_KeyDown;

            // Add mouse event handlers
            this.MouseWheel += Form1_MouseWheel;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.Focus();
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                //choose(false);
            }
            else if (e.Delta > 0)
            {
                //choose(true);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Subtract:
                case Keys.Escape:
                case Keys.F5:
                case Keys.NumPad0:
                    this.WindowState = FormWindowState.Minimized;
                    break;

                case Keys.C:
                    if (!string.IsNullOrWhiteSpace(label1.Text))
                        Clipboard.SetText(label1.Text);

                    e.Handled = true;
                    return;

                case Keys.R:
                case Keys.B:
                case Keys.Delete:
                    RemoveCurrentWord();
                    e.Handled = true;
                    return;

                case Keys.L:
                    LevelWordsTo0();
                    e.Handled = true;
                    return;

                // cycle languages with the backtick (`) / tilde (~) key
                case Keys.Oem3:
                    selectedLanguageIndex = (selectedLanguageIndex + 1) % languages.Count;
                    LoadWords();
                    e.Handled = true;
                    return;

                case Keys.OemMinus:
                    this.Opacity = Math.Max(0.01, this.Opacity - 0.01);
                    e.Handled = true;
                    return;

                case Keys.Oemplus:
                    this.Opacity = Math.Min(1.0, this.Opacity + 0.01);
                    e.Handled = true;
                    return;
                case Keys.J:
                    ChangeFontSize(-5);
                    e.Handled = true;
                    return;

                case Keys.K:
                    ChangeFontSize(5);
                    e.Handled = true;
                    return;

                case Keys.I:
                    int minStreak = words.Min(w => w.CorrectStreak);

                    int candidateCount = words.Count(w => w.CorrectStreak == minStreak);

                    MessageBox.Show(candidateCount.ToString());
                    e.Handled = true;
                    return;
                case Keys.PageDown:
                case Keys.Right:
                    choose(true);
                    e.Handled = true;
                    return;
                case Keys.PageUp:
                case Keys.Left:
                    choose(false);
                    e.Handled = true;
                    return;

            }


            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                int number = e.KeyCode - Keys.D0;

                if (number == 0)
                    this.Opacity = 1.0;           // 100%
                else
                    this.Opacity = 0.2 + number / 20.0;

                e.Handled = true;
                return;
            }
        }

        private void ChangeFontSize(float amount)
        {
            float newSize = label1.Font.Size + amount;

            // Prevent unusably small fonts
            if (newSize < 5)
                newSize = 5;

            label1.Font = new Font(
                label1.Font.FontFamily,
                newSize,
                label1.Font.Style);
        }

        private void LevelWordsTo0()
        {
            if (words == null || words.Count == 0 || currentLanguage == null) return;

            for (int i = 0; i < words.Count; i++)
            {
                var word = words[i];
                word.CorrectStreak = 1;

                var parts = allLines[word.RowIndex].Split(currentLanguage.Delimiter).ToList();

                while (parts.Count <= currentLanguage.StreakIndex)
                    parts.Add("");

                parts[currentLanguage.StreakIndex] = "1";

                allLines[word.RowIndex] = string.Join(currentLanguage.Delimiter, parts);
            }

            File.WriteAllLines(currentLanguage.FilePath, allLines);

            ShowNextWord();
        }

        private void RemoveCurrentWord()
        {
            if (currentWord == null) return;

            int rowToRemove = currentWord.RowIndex;

            // Remove from in-memory word list
            words.Remove(currentWord);

            // Remove from file lines
            var linesList = allLines.ToList();
            linesList.RemoveAt(rowToRemove);
            allLines = linesList.ToArray();

            // Fix RowIndex for remaining words
            foreach (var w in words)
            {
                if (w.RowIndex > rowToRemove)
                    w.RowIndex--;
            }

            // Rewrite file
            File.WriteAllLines(currentLanguage.FilePath, allLines);

            if (words.Count == 0)
            {
                label1.Text = "No words left";
                return;
            }

            ShowNextWord();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadWords();
        }

        private void LoadWords()
        {
            currentLanguage = languages[selectedLanguageIndex];
            label1.Font = new Font(label1.Font.FontFamily,currentLanguage.TextSize,label1.Font.Style);
            allLines = File.ReadAllLines(currentLanguage.FilePath);
            var list = new List<WordPair>();

            for (int i = 1; i < allLines.Length; i++)
            {
                var line = allLines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(currentLanguage.Delimiter);

                // determine required indices
                int maxIndex = Math.Max(currentLanguage.ForeignIndex, currentLanguage.EnglishIndex);
                if (currentLanguage.PronunciationIndex.HasValue)
                    maxIndex = Math.Max(maxIndex, currentLanguage.PronunciationIndex.Value);

                if (parts.Length <= maxIndex) continue;

                int streak = 0;
                if (parts.Length > currentLanguage.StreakIndex)
                {
                    if (int.TryParse(parts[currentLanguage.StreakIndex], out int parsed))
                        streak = parsed;
                }

                list.Add(new WordPair
                {
                    RowIndex = i,
                    foreign = parts[currentLanguage.ForeignIndex].Trim(),
                    pronunciation = currentLanguage.PronunciationIndex.HasValue && parts.Length > currentLanguage.PronunciationIndex.Value
                        ? parts[currentLanguage.PronunciationIndex.Value].Trim()
                        : string.Empty,
                    english = parts[currentLanguage.EnglishIndex].Trim(),
                    CorrectStreak = streak
                });
            }

            words = list;

            ShowNextWord();
        }


        private void ShowNextWord()
        {
            int minStreak = words.Min(w => w.CorrectStreak);

            var candidates = words
                .Where(w => w.CorrectStreak == minStreak)
                .ToList();

            WordPair nextWord;

            if (candidates.Count > 1)
            {
                do
                {
                    nextWord = candidates[rand.Next(candidates.Count)];
                }
                while (previousWord != null && nextWord == previousWord);
            }
            else
            {
                nextWord = candidates[0]; // Only one option available
            }

            currentWord = nextWord;
            previousWord = currentWord;

            label1.Text = currentWord.foreign;

            label2.Visible = false;

            completeAnswer = string.IsNullOrEmpty(currentWord.pronunciation)
            ? currentWord.english
            : $"{currentWord.pronunciation}{Environment.NewLine}{Environment.NewLine}{currentWord.english}";

            if (!currentLanguage.ShowPronunciationByDefault)
            {
                label2.Text = completeAnswer;
            }
            else if (!string.IsNullOrEmpty(currentWord.pronunciation))
            {
                label2.Text = currentWord.pronunciation;
                label2.Visible = true;
            }
            isClicked = false;

        }

        private void choose(bool correct)
        {
            if (!isClicked)
            {
                isClicked = true;
                label2.Text = completeAnswer;
                label2.Visible = true;
                return;
            }

            if (correct)
            {
                currentWord.CorrectStreak++;
            }

            SaveProgress(currentWord);
            ShowNextWord();
        }

        private void SaveProgress(WordPair word)
        {
            var parts = allLines[word.RowIndex].Split(currentLanguage.Delimiter).ToList();

            while (parts.Count <= currentLanguage.StreakIndex)
                parts.Add("");

            parts[currentLanguage.StreakIndex] = word.CorrectStreak.ToString();

            allLines[word.RowIndex] = string.Join(currentLanguage.Delimiter, parts);

            File.WriteAllLines(currentLanguage.FilePath, allLines);
        }

    }
}
