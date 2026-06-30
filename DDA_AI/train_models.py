import os

import pandas as pd

from models import KMeansModel

TRAIN_DATA_PATH = os.path.join("data", "transform", "output", "output_1782842417.csv")
MODELS_OUTPUT_PATH = os.path.join("output", "models")

if not os.path.exists(TRAIN_DATA_PATH):
    print(f"[ERROR] Train data not found, looking at path: {TRAIN_DATA_PATH}")
    exit(1)

try:
    df = pd.read_csv(TRAIN_DATA_PATH, parse_dates=["encounter_start_timestamp", "encounter_end_timestamp"])
except Exception as e:
    print("[ERROR] Could not load training data")
    print(e)
    exit(1)

try:
    # select the 7 last columns
    # https://stackoverflow.com/questions/33042633/selecting-last-n-columns-and-excluding-last-n-columns-in-dataframe
    filtered_data = df.iloc[:, -7:]

except Exception as e:
    print("[ERROR] Could not select correct data")
    print(e)
    exit(1)

try:
    test = KMeansModel(MODELS_OUTPUT_PATH)
    test.fit(filtered_data)
    test.print_model()
    test.save()
except Exception as e:
    print("[ERROR] Could not train KMeans model")
    print(e)
    exit(1)
