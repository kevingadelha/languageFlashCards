using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace languageFlashCards
{
    public partial class Form1 : Form
    {
        private class WordPair
        {
            public string japanese { get; set; }
            public string pronunciation { get; set; }
            public string translation { get; set; }
        }

        private List<WordPair> _words;
        private Random _rand = new Random();
        private WordPair _currentWord;
        private Label _correctLabel;
        private List<Label> _optionLabels = new List<Label>();
        private bool isClicked = false;
        private Color defaultColor = Color.Olive;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string path = @"C:\code\languageFlashCards\data\3000 common JP words - All.tsv";
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
            var lines = File.ReadAllLines(path);
            var list = new List<WordPair>();

            // Skip header line
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('\t');
                if (parts.Length < 5) continue; // #, week, day, japanese, pronunciation, translation...

                var wp = new WordPair
                {
                    japanese = parts[3].Trim(),
                    pronunciation = parts[4].Trim(),
                    translation = parts[5].Trim()
                };
                list.Add(wp);
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
            foreach (var lbl in _optionLabels)
                lbl.BackColor = defaultColor;

            _currentWord = _words[_rand.Next(_words.Count)];
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

            if (clicked == _correctLabel)
            {
                clicked.BackColor = Color.LightGreen;
            }
            else
            {
                _correctLabel.BackColor = Color.LightGreen;
                clicked.BackColor = Color.LightCoral;
            }
        }
    }
}
