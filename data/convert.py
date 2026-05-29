import pandas as pd
from pykakasi import kakasi

input_path = r"C:\temp\Optimized Kore.xlsx"
output_path = r"C:\temp\Optimized Kore_romaji.xlsx"

# load file
df = pd.read_excel(input_path)


source_col = df.columns[8]   # I column
target_col = df.columns[10]   # K column

kks = kakasi()
kks.setMode("H", "a")  # Hiragana -> ascii
kks.setMode("K", "a")  # Katakana -> ascii
kks.setMode("J", "a")  # Kanji -> ascii
kks.setMode("r", "Hepburn")
converter = kks.getConverter()

def to_romaji(text):
    if pd.isna(text):
        return text
    return converter.do(str(text))

# write into existing K column (overwrite)
df[target_col] = df[source_col].apply(to_romaji)

df.to_excel(output_path, index=False)

print("Done:", output_path)