from pathlib import Path

import aggregation_attributes as ag
from steps import DataPreparationStep

DATA_TO_TRANSFORM_PATH = Path("../data/transform")
OUTPUT_PATH = Path("../output")
OUTPUT_FILE = "data_preparation_bulk"

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

    path_to_save_next_outputs = ""
    for data_dir in data_dirs:
        data_preparation = DataPreparationStep(str(data_dir), str(OUTPUT_PATH), OUTPUT_FILE, attributes)
        print(f"Processing: {data_dir}")

        path_to_save_next_outputs = data_preparation.execute_all(output_custom_cols={"source_dir": data_dir},
                                                                 custom_output_path=path_to_save_next_outputs)

except OSError as e:
    print("Error: ", e)
