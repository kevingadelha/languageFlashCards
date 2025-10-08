using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace languageFlashCards
{
    public partial class Form1 : Form
    {
        private class WordPair
        {
            public string french { get; set; }
            public string english { get; set; }
        }

        private List<WordPair> _words;
        private Random _rand = new Random();
        private WordPair _currentWord;
        private Label _correctLabel;
        private List<Label> _optionLabels = new List<Label>();
        private Boolean isClicked = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MouseClick += Form1_NextWord;
            string path = @"C:\code\languageFlashCards\data\french_words.json";
            string json = File.ReadAllText(path);
            _words = JsonSerializer.Deserialize<List<WordPair>>(json);

            // First label is French prompt (label1)
            label1.Font = new Font(label1.Font.FontFamily, 24);
            label1.TextAlign = ContentAlignment.MiddleCenter;

            // Collect option labels (label2..label10)
            _optionLabels.AddRange(new[]
            {
                label2,label3,label4,label5,label6,label7,label8,label9,label10
            });

            foreach (var lbl in _optionLabels)
            {
                lbl.BackColor = SystemColors.Control;
                lbl.Click += Option_Click;
            }

            ShowNextWord();
        }

        private void ShowNextWord()
        {
            foreach (var lbl in _optionLabels)
                lbl.BackColor = SystemColors.Control;

            // Pick random word
            _currentWord = _words[_rand.Next(_words.Count)];
            label1.Text = _currentWord.french;

            // Pick correct position
            int correctIndex = _rand.Next(_optionLabels.Count);
            _correctLabel = _optionLabels[correctIndex];
            _correctLabel.Text = _currentWord.english;

            // Fill others with random wrong answers
            var used = new HashSet<int> { correctIndex };
            for (int i = 0; i < _optionLabels.Count; i++)
            {
                if (i == correctIndex) continue;

                int idx;
                do { idx = _rand.Next(_words.Count); }
                while (_words[idx].english == _currentWord.english);

                _optionLabels[i].Text = _words[idx].english;
            }
        }

        private void Option_Click(object sender, EventArgs e)
        {
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

        private void Form1_NextWord(object sender, MouseEventArgs e)
        {
            if (!isClicked) return;
            ShowNextWord();
        }
    }
}
