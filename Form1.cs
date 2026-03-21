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
            public int RowIndex { get; set; }   // line number in file (excluding header)
            public string japanese { get; set; }
            public string pronunciation { get; set; }
            public string translation { get; set; }
            public int CorrectStreak { get; set; }
        }


        private List<WordPair> _words;
        string path = @"C:\code\languageFlashCards\data\3000 common JP words - All.tsv";
        private string[] _allLines;
        private Random _rand = new Random();
        private WordPair _currentWord;
        private WordPair _previousWord;
        private bool isClicked = false;

        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true; // important to let the form capture key presses
            this.KeyDown += Form1_KeyDown;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Subtract:
                case Keys.Escape:
                case Keys.F5:
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
                    if (_words == null || _words.Count == 0) return;

                    foreach (var word in _words)
                    {
                        word.CorrectStreak = 0;

                        var parts = _allLines[word.RowIndex].Split('\t').ToList();

                        while (parts.Count <= 12)
                            parts.Add("");

                        parts[12] = "0";

                        _allLines[word.RowIndex] = string.Join("\t", parts);
                    }

                    File.WriteAllLines(path, _allLines);

                    ShowNextWord();
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
                    this.Opacity = number / 10.0; // 10% - 90%

                e.Handled = true;
                return;
            }
        }

        private void RemoveCurrentWord()
        {
            if (_currentWord == null) return;

            int rowToRemove = _currentWord.RowIndex;

            // Remove from in-memory word list
            _words.Remove(_currentWord);

            // Remove from file lines
            var linesList = _allLines.ToList();
            linesList.RemoveAt(rowToRemove);
            _allLines = linesList.ToArray();

            // Fix RowIndex for remaining words
            foreach (var w in _words)
            {
                if (w.RowIndex > rowToRemove)
                    w.RowIndex--;
            }

            // Rewrite file
            File.WriteAllLines(path, _allLines);

            if (_words.Count == 0)
            {
                label1.Text = "No words left";
                return;
            }

            ShowNextWord();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _words = LoadTsv(path);

            ShowNextWord();
        }

        private List<WordPair> LoadTsv(string path)
        {
            _allLines = File.ReadAllLines(path);
            var list = new List<WordPair>();

            for (int i = 1; i < _allLines.Length; i++) // skip header
            {
                var line = _allLines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split('\t');
                if (parts.Length < 6) continue;

                int streak = 0;

                if (parts.Length > 12 && int.TryParse(parts[12], out int parsed))
                    streak = parsed;

                list.Add(new WordPair
                {
                    RowIndex = i,
                    japanese = parts[3].Trim(),
                    pronunciation = parts[4].Trim(),
                    translation = parts[5].Trim(),
                    CorrectStreak = streak
                });
            }

            return list;
        }


        private void ShowNextWord()
        {
            int minStreak = _words.Min(w => w.CorrectStreak);

            var candidates = _words
                .Where(w => w.CorrectStreak == minStreak)
                .ToList();

            WordPair nextWord;

            if (candidates.Count > 1)
            {
                do
                {
                    nextWord = candidates[_rand.Next(candidates.Count)];
                }
                while (_previousWord != null && nextWord == _previousWord);
            }
            else
            {
                nextWord = candidates[0]; // Only one option available
            }

            _currentWord = nextWord;
            _previousWord = _currentWord;

            label1.Text = _currentWord.japanese;

            label2.Text =
                $"{_currentWord.pronunciation}{Environment.NewLine}{Environment.NewLine}{_currentWord.translation}";

            isClicked = false;
            label2.Visible = false;

        }

        private void choose(bool correct)
        {
            if (!isClicked)
            {
                isClicked = true;
                label2.Visible = true;
                return;
            }

            if (correct)
            {
                _currentWord.CorrectStreak++;
            }

            SaveProgress(_currentWord);
            ShowNextWord();
        }

        private void SaveProgress(WordPair word)
        {
            var parts = _allLines[word.RowIndex].Split('\t').ToList();

            while (parts.Count <= 12)
                parts.Add("");

            parts[12] = word.CorrectStreak.ToString();

            _allLines[word.RowIndex] = string.Join("\t", parts);

            File.WriteAllLines(path, _allLines);
        }

    }
}
