import os

import aggregation_attributes as ag
from steps import PreProcessingStep

TRANSFORM_DATA_PATH = os.path.join("data", "transform")
OUTPUT_PATH = os.path.join(TRANSFORM_DATA_PATH, "output")
OUTPUT_FILE = "output"

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
    print(f"Getting data to transform in path: {TRANSFORM_DATA_PATH}")
    data_dirs = [directory for directory in os.listdir(TRANSFORM_DATA_PATH) if
                 os.path.isdir(os.path.join(TRANSFORM_DATA_PATH, directory))]
    print(f"Directories detected: {data_dirs}")

    path_to_save_next_outputs = ""
    for data_dir in data_dirs:
        events_path = os.path.join(TRANSFORM_DATA_PATH, data_dir, "")
        pre_process = PreProcessingStep(events_path, OUTPUT_PATH, OUTPUT_FILE, attributes)
        print(f"Processing: {data_dir}")

        path_to_save_next_outputs = pre_process.execute_all(output_custom_cols={"source_dir": data_dir},
                                                            custom_output_path=path_to_save_next_outputs)

except OSError as e:
    print("Error: ", e)
