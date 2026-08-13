import pandas as pd
from pykakasi import kakasi

input_path = r"C:\temp\Japanese Core Vocab.tsv"
output_path = r"C:\temp\jp.tsv"

# Load TSV file
df = pd.read_csv(input_path, sep="\t", dtype=str, keep_default_na=False)

# Third column (0-based indexing)
target_col = df.columns[2]

# Set up pykakasi
kks = kakasi()
kks.setMode("H", "a")  # Hiragana -> romaji
kks.setMode("K", "a")  # Katakana -> romaji
kks.setMode("J", "a")  # Kanji -> romaji
kks.setMode("r", "Hepburn")
converter = kks.getConverter()

def to_romaji(text):
    if not text:
        return text
    return converter.do(text)

# Convert the third column in place
df[target_col] = df[target_col].apply(to_romaji)

# Save back to TSV
df.to_csv(output_path, sep="\t", index=False)

print("Done:", output_path)