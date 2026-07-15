from pathlib import Path

from fastapi import APIRouter

from dependencies import session_dependency, dda_pipeline_dependency
from requests import SetupSessionRequest

router = APIRouter()


@router.post("/session")
def setup_session(request: SetupSessionRequest, session_service: session_dependency,
                  dda_pipeline_service: dda_pipeline_dependency):
    if request.telemetry_base_path is not None:
        dda_pipeline_service.data_preparation.update_events_path(Path(request.telemetry_base_path))

    session_service.configure(request.session_id)
    return {
        "status": "success"
    }
