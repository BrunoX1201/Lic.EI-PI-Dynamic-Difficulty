from pathlib import Path
from typing import Annotated

from fastapi import Depends, HTTPException, Request

import aggregation_attributes as ag
from config import config
from models import KMeansModel, QLearningAgent
from services import Session, DDAPipeline
from steps import DataPreparationStep, DecisionStep

__ENVIRONMENT = config["ENVIRONMENT"]

# Session
__session = Session()


def get_session() -> Session:
    return __session


def verify_session(request: Request) -> None:
    if request.url.path != "/session" and not __session.is_configured:
        msg = "The session has not yet been configured."
        print(f"[ERROR] {msg}")
        raise HTTPException(status_code=409, detail=msg)


session_dependency = Annotated[Session, Depends(get_session)]

# DDA PIPELINE

## DATA PREPARATION STEP
__EVENT_BASE_PATH = Path(config["TELEMETRY_EVENTS_TESTING_PATH"] if __ENVIRONMENT == "dev" else "")
__ATTRIBUTES = [
    ag.AverageTimeBetweenKillsAttribute(),
    ag.EncounterTotalTimeAttribute(),
    ag.HasCompletedEncounterAttribute(),
    ag.PlayerAverageAccuracyAttribute(),
    ag.RemainingEnemiesAttribute(),
    ag.RemainingPlayerHealthAttribute(),
    ag.TotalHitsTakenAttribute(),
]
__data_preparation_step = DataPreparationStep(__EVENT_BASE_PATH, __ATTRIBUTES)

## DECISION STEP
__AVAILABLE_MODELS = []

if __ENVIRONMENT == "dev":
    __K_MEANS_MODEL_PATH = Path(config["K_MEANS_MODEL_PATH"])
    __k_model = KMeansModel()
    __k_model.load(__K_MEANS_MODEL_PATH)
    __AVAILABLE_MODELS.append(__k_model)

__q_agent = QLearningAgent()
__AVAILABLE_MODELS.append(__q_agent)

__decision_step = DecisionStep(__AVAILABLE_MODELS, __q_agent)

__dda_pipeline = DDAPipeline(__data_preparation_step, __decision_step)


def get_dda_pipeline() -> DDAPipeline:
    return __dda_pipeline


dda_pipeline_dependency = Annotated[DDAPipeline, Depends(get_dda_pipeline)]
