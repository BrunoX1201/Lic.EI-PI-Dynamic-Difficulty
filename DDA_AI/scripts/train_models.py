import datetime
import math
from pathlib import Path

import pandas as pd

from models import KMeansModel

TRAIN_DATASET_PATH = Path("../data/shared/data_preparation_bulk_1783292221.csv")
MODELS_OUTPUT_PATH = Path("../output/models")

if not Path.exists(TRAIN_DATASET_PATH):
    print(f"[ERROR] Train data not found, looking at path: {TRAIN_DATASET_PATH}")
    exit(1)

try:
    df = pd.read_csv(TRAIN_DATASET_PATH, parse_dates=["encounter_start_timestamp", "encounter_end_timestamp"])
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

k_means_model = KMeansModel()

models_to_train = [k_means_model]
for model in models_to_train:
    utc_date = datetime.datetime.now(datetime.timezone.utc)
    timestamp = math.floor(utc_date.timestamp())

    file_name = f"{model.name}_{timestamp}"
    try:
        print(f"[TRAINING] {model.name} model...", end="")
        model.fit(filtered_data)
        print("OK")
        model.print_model()
        model.save(MODELS_OUTPUT_PATH, file_name)

    except Exception as e:
        print("FAILED")
        print(f"[ERROR] Could not train {model.name} model")
        print(e)
