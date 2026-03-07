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
        private Label _correctLabel;
        private List<Label> _optionLabels = new List<Label>();
        private bool isClicked = false;
        private bool isShown = false;
        private bool showAll = false;
        private Color defaultColor = Color.Orange;
        private Color defaultText = Color.Black;

        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true; // important to let the form capture key presses
            this.KeyDown += Form1_KeyDown;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Subtract)
                this.WindowState = FormWindowState.Minimized;
            else if (e.KeyCode == Keys.C)
            {
                if (!string.IsNullOrWhiteSpace(label1.Text))
                    Clipboard.SetText(label1.Text);

                e.Handled = true;
                return;
            }
            else if (e.KeyCode == Keys.R)
            {
                RemoveCurrentWord();
                e.Handled = true;
                return;
            }
            else if (e.KeyCode == Keys.S)
            {
                showAll = !showAll;
                UpdateOptionVisibility();
                e.Handled = true;
                return;
            }
            /*else if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.B)
            {
                CenterCorrect();
                _correctLabel.Visible = true;
                isShown = true;
                e.Handled = true;
                return;
            }*/
            else if (e.KeyCode == Keys.L)
            {
                if (_words == null || _words.Count == 0) return;

                // Reset in-memory objects
                foreach (var word in _words)
                {
                    word.CorrectStreak = 0;

                    var parts = _allLines[word.RowIndex].Split('\t').ToList();

                    while (parts.Count <= 12)
                        parts.Add("");

                    parts[12] = "0";

                    _allLines[word.RowIndex] = string.Join("\t", parts);
                }

                // Rewrite file once (not per word)
                File.WriteAllLines(path, _allLines);

                // Optional: refresh word selection immediately
                ShowNextWord();
                e.Handled = true;
                return;
            }

            Label clickedLabel = e.KeyCode switch
                {
                    Keys.NumPad0 => label1,
                    Keys.PageUp => label1,
                    Keys.NumPad7 => label2,
                    Keys.NumPad8 => label3,
                    Keys.NumPad9 => label4,
                    Keys.NumPad4 => label5,
                    Keys.NumPad5 => label6,
                    Keys.NumPad6 => label7,
                    Keys.NumPad1 => label8,
                    Keys.NumPad2 => label9,
                    Keys.NumPad3 => label10,
                    Keys.Enter => _correctLabel,
                    Keys.PageDown => _correctLabel,
                    _ => null
                };

            if (clickedLabel != null)
            {
                Option_Click(clickedLabel, EventArgs.Empty);
                e.Handled = true;
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

            // Top "-" key → decrease 1%
            if (e.KeyCode == Keys.OemMinus)
            {
                this.Opacity = Math.Max(0.01, this.Opacity - 0.01);
                e.Handled = true;
                return;
            }

            // Top "=" key → increase 1%
            if (e.KeyCode == Keys.Oemplus)
            {
                this.Opacity = Math.Min(1.0, this.Opacity + 0.01);
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

        private void CenterCorrect()
        {
            label6.Text = _correctLabel.Text;
            _correctLabel = label6;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _words = LoadTsv(path);

            label1.Click += Global_Click;

            _optionLabels.AddRange(new[]
            {
                label2,label3,label4,label5,label6,label7,label8,label9,label10
            });

            foreach (var lbl in _optionLabels)
            {
                lbl.BackColor = defaultColor;
                lbl.Click += Option_Click;
                lbl.DoubleClick += Option_Click;
            }

            this.Click += Global_Click;

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


        private void Global_Click(object sender, EventArgs e)
        {
            if (!isClicked) return;
            ShowNextWord();
        }

        private void ShowNextWord()
        {
            label1.BackColor = defaultColor;
            label1.ForeColor = defaultText;
            foreach (var lbl in _optionLabels)
            {
                lbl.BackColor = defaultColor;
                lbl.ForeColor = defaultText;
            }

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

            int correctIndex = _rand.Next(_optionLabels.Count);
            _correctLabel = _optionLabels[correctIndex];
            _correctLabel.Text =
                $"{_currentWord.pronunciation}{Environment.NewLine}{Environment.NewLine}{_currentWord.translation}";

            // Track used word indices
            var usedWordIndexes = new HashSet<int>();

            // Add correct word index
            int correctWordIndex = _words.FindIndex(w => w.japanese == _currentWord.japanese);
            usedWordIndexes.Add(correctWordIndex);

            for (int i = 0; i < _optionLabels.Count; i++)
            {
                if (i == correctIndex) continue;

                int idx;
                do
                {
                    idx = _rand.Next(_words.Count);
                }
                while (usedWordIndexes.Contains(idx));

                usedWordIndexes.Add(idx);

                _optionLabels[i].Text =
                    $"{_words[idx].pronunciation}{Environment.NewLine}{Environment.NewLine}{_words[idx].translation}";
            }

            isClicked = false;
            isShown = false;
            UpdateOptionVisibility();

        }

        private void Option_Click(object sender, EventArgs e)
        {
            if (isClicked && showAll)
            {
                ShowNextWord();
                return;
            }
            var clicked = sender as Label;
            if (clicked == null) return;

            bool correct = clicked == _correctLabel;
            if (!showAll && _correctLabel != null && !isClicked)
            {
                isClicked = true;
                CenterCorrect();
                _correctLabel.Visible = true;
                return;
            }
            isClicked = true;

            if (correct)
            {
                if (showAll)
                {
                    _correctLabel.ForeColor = Color.LightGreen;
                    clicked.ForeColor = Color.LightGreen;
                    label1.ForeColor = Color.LightGreen;
                }
                _currentWord.CorrectStreak++;
            }
            else
            {
                if (showAll)
                {
                    _correctLabel.ForeColor = Color.LightGreen;
                    clicked.ForeColor = Color.LightCoral;
                    label1.ForeColor = Color.LightCoral;
                }
            }

            SaveProgress(_currentWord);
            if (!showAll && isClicked)
            {
                ShowNextWord();
            }
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
        private void UpdateOptionVisibility()
        {
            if (showAll)
            {
                foreach (var lbl in _optionLabels)
                    lbl.Visible = true;
            }
            else
            {
                foreach (var lbl in _optionLabels)
                    lbl.Visible = false;

                // Always show correct label if answer has been revealed
                if (isClicked && _correctLabel != null)
                    _correctLabel.Visible = true;
            }
        }

    }
}
