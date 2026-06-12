import aggregation_attributes as ag

from steps import PreProcessingStep

attributes = [ag.AverageTimeBetweenKillsAttribute(),
              ag.EncounterTotalTimeAttribute(),
              ag.HasCompletedEncounterAttribute(),
              ag.PlayerAverageAccuracyAttribute(),
              ag.RemainingEnemiesAttribute(),
              ag.RemainingPlayerHealthAttribute(),
              ag.TotalHitsTakenAttribute()
              ]
pre_process = PreProcessingStep("./testdata/", "./output", "test", attributes)

pre_process.transform("01-002")
pre_process.save_output()

pre_process.transform("01-010")
pre_process.save_output()
