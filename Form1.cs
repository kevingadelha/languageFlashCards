using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace languageFlashCards
{
    //Add transparency
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
        private Label _correctLabel;
        private List<Label> _optionLabels = new List<Label>();
        private bool isClicked = false;
        private Color defaultColor = Color.White;

        public Form1()
        {
            InitializeComponent();
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
            foreach (var lbl in _optionLabels)
                lbl.BackColor = defaultColor;

            int minStreak = _words.Min(w => w.CorrectStreak);

            var candidates = _words
                .Where(w => w.CorrectStreak == minStreak)
                .ToList();

            _currentWord = candidates[_rand.Next(candidates.Count)];

            label1.Text = _currentWord.japanese;

            int correctIndex = _rand.Next(_optionLabels.Count);
            _correctLabel = _optionLabels[correctIndex];
            _correctLabel.Text = $"{_currentWord.pronunciation}{Environment.NewLine}{Environment.NewLine}{_currentWord.translation}";

            var used = new HashSet<int> { correctIndex };
            for (int i = 0; i < _optionLabels.Count; i++)
            {
                if (i == correctIndex) continue;

                int idx;
                do { idx = _rand.Next(_words.Count); }
                while (_words[idx].japanese == _currentWord.japanese);

                _optionLabels[i].Text = $"{_words[idx].pronunciation}{Environment.NewLine}{Environment.NewLine}{_words[idx].translation}";
            }

            isClicked = false;
        }

        private void Option_Click(object sender, EventArgs e)
        {
            if (isClicked)
            {
                ShowNextWord();
                return;
            }
            isClicked = true;
            var clicked = sender as Label;
            if (clicked == null) return;

            bool correct = clicked == _correctLabel;

            if (correct)
            {
                clicked.BackColor = Color.LightGreen;
                label1.BackColor = Color.LightGreen;
                _currentWord.CorrectStreak++;
            }
            else
            {
                _correctLabel.BackColor = Color.LightGreen;
                clicked.BackColor = Color.LightCoral;
                label1.BackColor = Color.LightCoral;
                _currentWord.CorrectStreak = 0;
            }

            SaveProgress(_currentWord);

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
