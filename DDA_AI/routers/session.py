from pathlib import Path

from fastapi import APIRouter

from config import config
from dependencies import session_dependency, dda_pipeline_dependency
from requests import SetupSessionRequest

router = APIRouter()


@router.post("/session")
def setup_session(request: SetupSessionRequest, session_service: session_dependency,
                  dda_pipeline_service: dda_pipeline_dependency):
    new_telemetry_path = ""
    if request.telemetry_base_path is not None and request.telemetry_base_path != "":
        new_telemetry_path = request.telemetry_base_path
    elif config["ENVIRONMENT"] == "dev":
        new_telemetry_path = config["TELEMETRY_EVENTS_TESTING_PATH"]

    dda_pipeline_service.data_preparation.update_events_path(Path(new_telemetry_path))
    session_service.configure(request.session_id)
    return {
        "status": "success"
    }
