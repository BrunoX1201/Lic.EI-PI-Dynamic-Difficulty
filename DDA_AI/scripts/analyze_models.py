from pathlib import Path

import pandas as pd

from models import KMeansModel

SHARED_MODELS_PATH = Path("../data/shared")
K_MEANS_DATASET_PATH = Path(f"{SHARED_MODELS_PATH}/data_preparation_bulk_1783292221.csv")

# (dda_model, model_saved_path, train_dataset)
MODELS_TO_ANALYZE = [
    (KMeansModel(), Path(f"{SHARED_MODELS_PATH}/k-means_1783292308.pkl"), K_MEANS_DATASET_PATH),
    (KMeansModel(), Path(f"{SHARED_MODELS_PATH}/k-means_1783867513.pkl"), K_MEANS_DATASET_PATH)
]

for i, item in enumerate(MODELS_TO_ANALYZE):
    try:
        print(f"Analyzing model #{i} ({item[0].name})")
        item[0].load(item[1])
        item[0].print_model()

        train_dataset = item[2]
        if train_dataset != "":
            df = pd.read_csv(train_dataset, parse_dates=["encounter_start_timestamp", "encounter_end_timestamp"])
            filtered_data = df.iloc[:, -7:]
            item[0].print_scores(filtered_data)

    except Exception as e:
        print(e)
