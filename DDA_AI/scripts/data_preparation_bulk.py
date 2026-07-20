import datetime
import math
from pathlib import Path

import aggregation_attributes as ag
from steps import DataPreparationStep

DATA_TO_TRANSFORM_PATH = Path("../data/transform")
OUTPUT_PATH = Path("../output")

utc_date = datetime.datetime.now(datetime.timezone.utc)
timestamp = math.floor(utc_date.timestamp())
OUTPUT_FILE = f"data_preparation_bulk_{timestamp}"

attributes = [
    ag.AverageTimeBetweenKillsAttribute(),
    ag.EncounterTotalTimeAttribute(),
    ag.HasCompletedEncounterAttribute(),
    ag.PlayerAverageAccuracyAttribute(),
    ag.RemainingEnemiesAttribute(),
    ag.RemainingPlayerHealthAttribute(),
    ag.TotalHitsTakenAttribute(),
]

try:
    print(f"Getting data to transform in path: {DATA_TO_TRANSFORM_PATH}")
    data_dirs = [directory for directory in DATA_TO_TRANSFORM_PATH.iterdir() if
                 directory.is_dir()]
    print(f"Directories detected: {data_dirs}")

    for data_dir in data_dirs:
        data_preparation = DataPreparationStep(data_dir, attributes)
        print(f"Processing: {data_dir}")
        data_preparation.execute_all(OUTPUT_PATH, OUTPUT_FILE, {"source_dir": str(data_dir)})

except OSError as e:
    print("Error: ", e)
