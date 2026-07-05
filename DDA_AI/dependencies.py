from pathlib import Path
from typing import Annotated

from fastapi import Depends

import aggregation_attributes as ag
from services import Session, DDAPipeline
from steps import DataPreparationStep

# Session
__session = Session()


def get_session() -> Session:
    return __session


session_dependency = Annotated[Session, Depends(get_session)]

# DDA PIPELINE
__EVENT_BASE_PATH = Path("./data/test/")
__ATTRIBUTES = [
    ag.AverageTimeBetweenKillsAttribute(),
    ag.EncounterTotalTimeAttribute(),
    ag.HasCompletedEncounterAttribute(),
    ag.PlayerAverageAccuracyAttribute(),
    ag.RemainingEnemiesAttribute(),
    ag.RemainingPlayerHealthAttribute(),
    ag.TotalHitsTakenAttribute(),
]
__data_preparation = DataPreparationStep(__EVENT_BASE_PATH, __ATTRIBUTES)

__dda_pipeline = DDAPipeline(__data_preparation)


def get_dda_pipeline() -> DDAPipeline:
    return __dda_pipeline


dda_pipeline_dependency = Annotated[DDAPipeline, Depends(get_dda_pipeline)]
